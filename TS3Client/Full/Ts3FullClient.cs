// TS3Client - A free TeamSpeak3 client implementation
// Copyright (C) 2017  TS3Client contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the Open Software License v. 3.0
//
// You should have received a copy of the Open Software License along with this
// program. If not, see <https://opensource.org/licenses/OSL-3.0>.

namespace TS3Client.Full
{
	using Audio;
	using Commands;
	using Helper;
	using Messages;
	using System;
	using System.Buffers.Binary;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using ChannelIdT = System.UInt64;
	using ClientIdT = System.UInt16;
	using CmdR = System.E<Messages.CommandError>;

	/// <summary>Creates a full TeamSpeak3 client with voice capabilities.</summary>
	public sealed partial class Ts3FullClient : Ts3BaseFunctions, IAudioActiveProducer, IAudioPassiveConsumer
	{
		private Ts3Crypt ts3Crypt;
		private PacketHandler<S2C, C2S> packetHandler;
		private readonly AsyncMessageProcessor msgProc;

		private readonly object statusLock = new object();

		private int returnCode;
		private ConnectionContext context;

		private IEventDispatcher dispatcher;
		public override ClientType ClientType => ClientType.Full;
		/// <summary>The client id given to this connection by the server.</summary>
		public ushort ClientId => packetHandler.ClientId;
		/// <summary>The disonnect message when leaving.</summary>
		public string QuitMessage { get; set; } = "Disconnected";
		/// <summary>The <see cref="Full.VersionSign"/> used to connect.</summary>
		public VersionSign VersionSign { get; private set; }
		/// <summary>The <see cref="Full.IdentityData"/> used to connect.</summary>
		public IdentityData Identity => ts3Crypt.Identity;
		private Ts3ClientStatus status;
		public override bool Connected { get { lock (statusLock) return status == Ts3ClientStatus.Connected; } }
		public override bool Connecting { get { lock (statusLock) return status == Ts3ClientStatus.Connecting; } }
		protected override Deserializer Deserializer => msgProc.Deserializer;
		private ConnectionDataFull connectionDataFull;
		public Book.Connection Book { get; set; } = new Book.Connection();

		public override event EventHandler<EventArgs> OnConnected;
		public override event EventHandler<DisconnectEventArgs> OnDisconnected;
		public event EventHandler<CommandError> OnErrorEvent;

		/// <summary>Creates a new client. A client can manage one connection to a server.</summary>
		/// <param name="dispatcherType">The message processing method for incomming notifications.
		/// See <see cref="EventDispatchType"/> for further information about each type.</param>
		public Ts3FullClient()
		{
			status = Ts3ClientStatus.Disconnected;
			msgProc = new AsyncMessageProcessor(MessageHelper.GetToClientNotificationType);
			context = new ConnectionContext { WasExit = true };
		}

		/// <summary>Tries to connect to a server.</summary>
		/// <param name="conData">Set the connection information properties as needed.
		/// For further details about each setting see the respective property documentation in <see cref="ConnectionData"/></param>
		/// <exception cref="ArgumentException">When some required values are not set or invalid.</exception>
		/// <exception cref="Ts3Exception">When the connection could not be established.</exception>
		public override void Connect(ConnectionData conData)
		{
			if (!(conData is ConnectionDataFull conDataFull)) throw new ArgumentException($"Use the {nameof(ConnectionDataFull)} derivative to connect with the full client.", nameof(conData));
			if (conDataFull.Identity is null) throw new ArgumentNullException(nameof(conDataFull.Identity));
			if (conDataFull.VersionSign is null) throw new ArgumentNullException(nameof(conDataFull.VersionSign));
			connectionDataFull = conDataFull;
			ConnectionData = conData;

			Disconnect();

			if (!TsDnsResolver.TryResolve(conData.Address, out remoteAddress))
				throw new Ts3Exception("Could not read or resolve address.");

			lock (statusLock)
			{
				returnCode = 0;
				status = Ts3ClientStatus.Connecting;

				VersionSign = conDataFull.VersionSign;
				ts3Crypt = new Ts3Crypt();
				ts3Crypt.Identity = conDataFull.Identity;

				var ctx = new ConnectionContext { WasExit = false };
				context = ctx;

				packetHandler = new PacketHandler<S2C, C2S>(ts3Crypt, conData.LogId);
				packetHandler.PacketEvent = (ref Packet<S2C> packet) => { PacketEvent(ctx, ref packet); };
				packetHandler.StopEvent = (closeReason) => { ctx.ExitReason = closeReason; DisconnectInternal(ctx, setStatus: Ts3ClientStatus.Disconnected); };
				packetHandler.Connect(remoteAddress);
				dispatcher = new ExtraThreadEventDispatcher();
				dispatcher.Init(InvokeEvent, conData.LogId);
			}
		}

		/// <summary>
		/// Disconnects from the current server and closes the connection.
		/// Does nothing if the client is not connected.
		/// </summary>
		public override void Disconnect()
		{
			DisconnectInternal(context);
			while (true)
			{
				if (context.WasExit)
					break;
				dispatcher.DoWork();
				if (!context.WasExit)
					Thread.Sleep(1);
			}
		}

		private void DisconnectInternal(ConnectionContext ctx, CommandError error = null, Ts3ClientStatus? setStatus = null)
		{
			bool triggerEventSafe = false;

			lock (statusLock)
			{
				Log.Debug("DisconnectInternal wasExit:{0} error:{1} oldStatus:{2} newStatus:{3}", ctx.WasExit, error?.ErrorFormat(), status, setStatus);

				if (setStatus.HasValue)
					status = setStatus.Value;

				if (ctx.WasExit)
					return;

				switch (status)
				{
				case Ts3ClientStatus.Connecting:
				case Ts3ClientStatus.Disconnected:
					ctx.WasExit = true;
					packetHandler.Stop();
					msgProc.DropQueue();
					dispatcher.Dispose();
					dispatcher = null;
					triggerEventSafe = true;
					break;
				case Ts3ClientStatus.Disconnecting:
					break;
				case Ts3ClientStatus.Connected:
					ClientDisconnect(Reason.LeftServer, QuitMessage);
					status = Ts3ClientStatus.Disconnecting;
					break;
				default:
					throw Util.UnhandledDefault(status);
				}
			}

			if (triggerEventSafe)
				OnDisconnected?.Invoke(this, new DisconnectEventArgs(ctx.ExitReason ?? Reason.LeftServer, error));
		}

		private void PacketEvent(ConnectionContext ctx, ref Packet<S2C> packet)
		{
			lock (statusLock)
			{
				if (ctx.WasExit)
					return;

				switch (packet.PacketType)
				{
				case PacketType.Command:
				case PacketType.CommandLow:
					Log.ConditionalDebug("[I] {0}", Util.Encoder.GetString(packet.Data));
					var result = msgProc.PushMessage(packet.Data);
					if (result.HasValue)
						dispatcher.Invoke(result.Value);
					break;

				case PacketType.Voice:
				case PacketType.VoiceWhisper:
					OutStream?.Write(packet.Data, new Meta
					{
						In = new MetaIn
						{
							Whisper = packet.PacketType == PacketType.VoiceWhisper
						}
					});
					break;

				case PacketType.Init1:
					// Init error
					if (packet.Data.Length == 5 && packet.Data[0] == 1)
					{
						var errorNum = BinaryPrimitives.ReadUInt32LittleEndian(packet.Data.AsSpan(1));
						if (Enum.IsDefined(typeof(Ts3ErrorCode), errorNum))
							Log.Info("Got init error: {0}", (Ts3ErrorCode)errorNum);
						else
							Log.Warn("Got undefined init error: {0}", errorNum);
						DisconnectInternal(ctx, setStatus: Ts3ClientStatus.Disconnected);
					}
					break;
				}
			}
		}

		// Local event processing

		partial void ProcessEachInitIvExpand(InitIvExpand initIvExpand)
		{
			packetHandler.ReceivedFinalInitAck();

			var result = ts3Crypt.CryptoInit(initIvExpand.Alpha, initIvExpand.Beta, initIvExpand.Omega);
			if (!result)
			{
				DisconnectInternal(context, Util.CustomError($"Failed to calculate shared secret: {result.Error}"));
				return;
			}

			DefaultClientInit();
		}

		partial void ProcessEachInitIvExpand2(InitIvExpand2 initIvExpand2)
		{
			packetHandler.ReceivedFinalInitAck();

			var (publicKey, privateKey) = Ts3Crypt.GenerateTemporaryKey();

			var ekBase64 = Convert.ToBase64String(publicKey);
			var toSign = new byte[86];
			Array.Copy(publicKey, 0, toSign, 0, 32);
			var beta = Convert.FromBase64String(initIvExpand2.Beta);
			Array.Copy(beta, 0, toSign, 32, 54);
			var sign = Ts3Crypt.Sign(connectionDataFull.Identity.PrivateKey, toSign);
			var proof = Convert.ToBase64String(sign);
			ClientEk(ekBase64, proof);

			var result = ts3Crypt.CryptoInit2(initIvExpand2.License, initIvExpand2.Omega, initIvExpand2.Proof, initIvExpand2.Beta, privateKey);
			if (!result)
			{
				DisconnectInternal(context, Util.CustomError($"Failed to calculate shared secret: {result.Error}"));
				return;
			}

			DefaultClientInit();
		}

		partial void ProcessEachInitServer(InitServer initServer)
		{
			packetHandler.ClientId = initServer.ClientId;

			lock (statusLock)
				status = Ts3ClientStatus.Connected;
			OnConnected?.Invoke(this, EventArgs.Empty);
		}

		partial void ProcessEachPluginCommand(PluginCommand cmd)
		{
			if (cmd.Name == "cliententerview" && cmd.Data == "version")
				SendPluginCommand("cliententerview", "TAB", PluginTargetMode.Server);
		}

		partial void ProcessEachCommandError(CommandError error)
		{
			bool skipError = false;
			bool disconnect = false;
			lock (statusLock)
			{
				if (status == Ts3ClientStatus.Connecting)
				{
					disconnect = true;
					skipError = true;
				}
			}

			if (disconnect)
				DisconnectInternal(context, error, Ts3ClientStatus.Disconnected);
			if (!skipError)
				OnErrorEvent?.Invoke(this, error);
		}

		partial void ProcessEachClientLeftView(ClientLeftView clientLeftView)
		{
			if (clientLeftView.ClientId == packetHandler.ClientId)
			{
				context.ExitReason = Reason.LeftServer;
				DisconnectInternal(context, setStatus: Ts3ClientStatus.Disconnected);
			}
		}

		partial void ProcessEachChannelListFinished(ChannelListFinished _)
		{
			ChannelSubscribeAll();
			PermissionList();
		}

		partial void ProcessEachClientConnectionInfoUpdateRequest(ClientConnectionInfoUpdateRequest _)
		{
			SendNoResponsed(packetHandler.NetworkStats.GenerateStatusAnswer());
		}

		partial void ProcessPermList(PermList[] permList)
		{
			var buildPermissions = new List<Ts3Permission>(permList.Length + 1) { Ts3Permission.undefined };
			foreach (var perm in permList)
			{
				if (!string.IsNullOrEmpty(perm.PermissionName))
				{
					if (Enum.TryParse<Ts3Permission>(perm.PermissionName, out var ts3perm))
						buildPermissions.Add(ts3perm);
					else
						buildPermissions.Add(Ts3Permission.undefined);
				}
			}
			Deserializer.PermissionTransform = new TablePermissionTransform(buildPermissions.ToArray());
		}

		// ***

		private CmdR DefaultClientInit() => ClientInit(
			connectionDataFull.Username,
			true, true,
			connectionDataFull.DefaultChannel,
			connectionDataFull.DefaultChannelPassword.HashedPassword,
			connectionDataFull.ServerPassword.HashedPassword,
			string.Empty, string.Empty, string.Empty,
			connectionDataFull.Identity.ClientUid, VersionSign);

		/// <summary>
		/// Sends a command to the server. Commands look exactly like query commands and mostly also behave identically.
		/// <para>NOTE: Do not expect all commands to work exactly like in the query documentation.</para>
		/// </summary>
		/// <typeparam name="T">The type to deserialize the response to. Use <see cref="ResponseDictionary"/> for unknow response data.</typeparam>
		/// <param name="com">The command to send.
		/// <para>NOTE: By default does the command expect an answer from the server. Set <see cref="Ts3Command.ExpectResponse"/> to false
		/// if the client hangs after a special command (<see cref="Send{T}(Ts3Command)"/> will return a generic error instead).</para></param>
		/// <returns>Returns <code>R(OK)</code> with an enumeration of the deserialized and split up in <see cref="T"/> objects data.
		/// Or <code>R(ERR)</code> with the returned error if no response is expected.</returns>
		public override R<T[], CommandError> Send<T>(Ts3Command com)
		{
			using (var wb = new WaitBlock(msgProc.Deserializer, false))
			{
				var result = SendCommandBase(wb, com);
				if (!result.Ok)
					return result.Error;
				if (com.ExpectResponse)
					return wb.WaitForMessage<T>();
				else
					return Array.Empty<T>();
			}
		}

		/// <summary>
		/// Sends a command without expecting a 'error' return code.
		/// <para>NOTE: Do not use this method unless you are sure the ts3 command fits the criteria.</para>
		/// </summary>
		/// <param name="command">The command to send.</param>
		public CmdR SendNoResponsed(Ts3Command command)
			=> Send<ResponseVoid>(command.ExpectsResponse(false));

		public override R<T[], CommandError> SendHybrid<T>(Ts3Command com, NotificationType type)
			=> SendNotifyCommand(com, type).UnwrapNotification<T>();

		public R<LazyNotification, CommandError> SendNotifyCommand(Ts3Command com, params NotificationType[] dependsOn)
		{
			if (!com.ExpectResponse)
				throw new ArgumentException("A special command must take a response");

			using (var wb = new WaitBlock(msgProc.Deserializer, false, dependsOn))
			{
				var result = SendCommandBase(wb, com);
				if (!result.Ok)
					return result.Error;
				return wb.WaitForNotification();
			}
		}

		private E<CommandError> SendCommandBase(WaitBlock wb, Ts3Command com)
		{
			lock (statusLock)
			{
				if (context.WasExit || (!Connected && com.ExpectResponse))
					return Util.TimeOutCommandError;

				if (com.ExpectResponse)
				{
					var responseNumber = ++returnCode;
					var retCodeParameter = new CommandParameter("return_code", responseNumber);
					com.Add(retCodeParameter);
					msgProc.EnqueueRequest(retCodeParameter.Value, wb);
				}

				var message = com.ToString();
				Log.Debug("[O] {0}", message);
				byte[] data = Util.Encoder.GetBytes(message);
				packetHandler.AddOutgoingPacket(data, PacketType.Command);
			}
			return R.Ok;
		}

		public async Task<R<T[], CommandError>> SendCommandAsync<T>(Ts3Command com) where T : IResponse, new()
		{
			using (var wb = new WaitBlock(msgProc.Deserializer, true))
			{
				var result = SendCommandBase(wb, com);
				if (!result.Ok)
					return result.Error;
				if (com.ExpectResponse)
					return await wb.WaitForMessageAsync<T>().ConfigureAwait(false);
				else
					// This might not be the nicest way to return in this case
					// but we don't know what the response is, so this acceptable.
					return Util.NoResultCommandError;
			}
		}

		/// <summary>Release all resources. Will try to disconnect before disposing.</summary>
		public override void Dispose()
		{
			Disconnect();
		}

		#region Audio
		/// <summary>Receive voice packets.</summary>
		public IAudioPassiveConsumer OutStream { get; set; }
		/// <summary>When voice data can be sent.</summary>
		// TODO may set to false if no talk power, etc.
		public bool Active => true;
		/// <summary>Send voice data.</summary>
		/// <param name="data">The encoded audio buffer.</param>
		/// <param name="meta">The metadata where to send the packet.</param>
		public void Write(Span<byte> data, Meta meta)
		{
			if (meta.Out is null
				|| meta.Out.SendMode == TargetSendMode.None
				|| !meta.Codec.HasValue
				|| meta.Codec.Value == Codec.Raw)
				return;

			switch (meta.Out.SendMode)
			{
			case TargetSendMode.None:
				break;
			case TargetSendMode.Voice:
				SendAudio(data, meta.Codec.Value);
				break;
			case TargetSendMode.Whisper:
				SendAudioWhisper(data, meta.Codec.Value, meta.Out.ChannelIds, meta.Out.ClientIds);
				break;
			case TargetSendMode.WhisperGroup:
				SendAudioGroupWhisper(data, meta.Codec.Value, meta.Out.GroupWhisperType, meta.Out.GroupWhisperTarget, meta.Out.TargetId);
				break;
			default: throw new ArgumentOutOfRangeException(nameof(meta.Out.SendMode), meta.Out.SendMode, "SendMode not handled");
			}
		}
		#endregion

		#region FULLCLIENT SPECIFIC COMMANDS

		public CmdR ChangeIsChannelCommander(bool isChannelCommander)
			=> Send<ResponseVoid>(new Ts3Command("clientupdate") {
				{ "client_is_channel_commander", isChannelCommander },
			});

		public CmdR RequestTalkPower(string message = null)
			=> Send<ResponseVoid>(new Ts3Command("clientupdate") {
				{ "client_talk_request", true },
				{ "client_talk_request_msg", message },
			});

		public CmdR CancelTalkPowerRequest()
			=> Send<ResponseVoid>(new Ts3Command("clientupdate") {
				{ "client_talk_request", false },
			});

		public CmdR ClientEk(string ek, string proof)
			=> SendNoResponsed(new Ts3Command("clientek") {
				{ "ek", ek },
				{ "proof", proof },
			});

		public CmdR ClientInit(string nickname, bool inputHardware, bool outputHardware,
				string defaultChannel, string defaultChannelPassword, string serverPassword, string metaData,
				string nicknamePhonetic, string defaultToken, string hwid, VersionSign versionSign)
			=> SendNoResponsed(new Ts3Command("clientinit") {
				{ "client_nickname", nickname },
				{ "client_version", versionSign.Name },
				{ "client_platform", versionSign.PlatformName },
				{ "client_input_hardware", inputHardware },
				{ "client_output_hardware", outputHardware },
				{ "client_default_channel", defaultChannel },
				{ "client_default_channel_password", defaultChannelPassword }, // base64(sha1(pass))
				{ "client_server_password", serverPassword }, // base64(sha1(pass))
				{ "client_meta_data", metaData },
				{ "client_version_sign", versionSign.Sign },
				{ "client_key_offset", Identity.ValidKeyOffset },
				{ "client_nickname_phonetic", nicknamePhonetic },
				{ "client_default_token", defaultToken },
				{ "hwid", hwid },
			});

		public CmdR ClientDisconnect(Reason reason, string reasonMsg)
			=> SendNoResponsed(new Ts3Command("clientdisconnect") {
				{ "reasonid", (int)reason },
				{ "reasonmsg", reasonMsg }
			});

		public CmdR ChannelSubscribeAll()
			=> Send<ResponseVoid>(new Ts3Command("channelsubscribeall"));

		public CmdR ChannelUnsubscribeAll()
			=> Send<ResponseVoid>(new Ts3Command("channelunsubscribeall"));

		public CmdR PokeClient(string message, ClientIdT clientId)
			=> SendNoResponsed(new Ts3Command("clientpoke") {
				{ "clid", clientId },
				{ "msg", message },
			});

		public void SendAudio(in ReadOnlySpan<byte> data, Codec codec)
		{
			// [X,X,Y,DATA]
			// > X is a ushort in H2N order of an own audio packet counter
			//     it seems it can be the same as the packet counter so we will let the packethandler do it.
			// > Y is the codec byte (see Enum)
			Span<byte> tmpBuffer = stackalloc byte[data.Length + 3];
			tmpBuffer[2] = (byte)codec;
			data.CopyTo(tmpBuffer.Slice(3));

			packetHandler.AddOutgoingPacket(tmpBuffer, PacketType.Voice);
		}

		public void SendAudioWhisper(in ReadOnlySpan<byte> data, Codec codec, IReadOnlyList<ChannelIdT> channelIds, IReadOnlyList<ClientIdT> clientIds)
		{
			// [X,X,Y,N,M,(U,U,U,U,U,U,U,U)*,(T,T)*,DATA]
			// > X is a ushort in H2N order of an own audio packet counter
			//     it seems it can be the same as the packet counter so we will let the packethandler do it.
			// > Y is the codec byte (see Enum)
			// > N is a byte, the count of ChannelIds to send to
			// > M is a byte, the count of ClientIds to send to
			// > U is a ulong in H2N order of each targeted channelId, (U...U) is repeated N times
			// > T is a ushort in H2N order of each targeted clientId, (T...T) is repeated M times
			int offset = 2 + 1 + 2 + channelIds.Count * 8 + clientIds.Count * 2;
			Span<byte> tmpBuffer = stackalloc byte[data.Length + offset];
			tmpBuffer[2] = (byte)codec;
			tmpBuffer[3] = (byte)channelIds.Count;
			tmpBuffer[4] = (byte)clientIds.Count;
			for (int i = 0; i < channelIds.Count; i++)
				BinaryPrimitives.WriteUInt64BigEndian(tmpBuffer.Slice(5 + (i * 8)), channelIds[i]);
			for (int i = 0; i < clientIds.Count; i++)
				BinaryPrimitives.WriteUInt16BigEndian(tmpBuffer.Slice(5 + channelIds.Count * 8 + (i * 2)), clientIds[i]);
			data.CopyTo(tmpBuffer.Slice(offset));

			packetHandler.AddOutgoingPacket(tmpBuffer, PacketType.VoiceWhisper);
		}

		public void SendAudioGroupWhisper(in ReadOnlySpan<byte> data, Codec codec, GroupWhisperType type, GroupWhisperTarget target, ulong targetId = 0)
		{
			// [X,X,Y,N,M,U,U,U,U,U,U,U,U,DATA]
			// > X is a ushort in H2N order of an own audio packet counter
			//     it seems it can be the same as the packet counter so we will let the packethandler do it.
			// > Y is the codec byte (see Enum)
			// > N is a byte, specifying the GroupWhisperType
			// > M is a byte, specifying the GroupWhisperTarget
			// > U is a ulong in H2N order for the targeted channelId or groupId (0 if not applicable)
			Span<byte> tmpBuffer = stackalloc byte[data.Length + 13];
			tmpBuffer[2] = (byte)codec;
			tmpBuffer[3] = (byte)type;
			tmpBuffer[4] = (byte)target;
			BinaryPrimitives.WriteUInt64BigEndian(tmpBuffer.Slice(5), targetId);
			data.CopyTo(tmpBuffer.Slice(13));

			packetHandler.AddOutgoingPacket(tmpBuffer, PacketType.VoiceWhisper, PacketFlags.Newprotocol);
		}

		public R<ClientConnectionInfo, CommandError> GetClientConnectionInfo(ClientIdT clientId)
		{
			var result = SendNotifyCommand(new Ts3Command("getconnectioninfo") {
				{ "clid", clientId }
			}, NotificationType.ClientConnectionInfo);
			if (!result.Ok)
				return result.Error;
			return result.Value.Notifications
				.Cast<ClientConnectionInfo>()
				.Where(x => x.ClientId == clientId)
				.WrapSingle();
		}

		public R<ClientUpdated, CommandError> GetClientVariables(ClientIdT clientId)
			=> SendNotifyCommand(new Ts3Command("clientgetvariables") {
				{ "clid", clientId }
			}, NotificationType.ClientUpdated).UnwrapNotification<ClientUpdated>().WrapSingle();

		public R<ServerUpdated, CommandError> GetServerVariables()
			=> SendNotifyCommand(new Ts3Command("servergetvariables"),
				NotificationType.ServerUpdated).UnwrapNotification<ServerUpdated>().WrapSingle();

		public CmdR SendPluginCommand(string name, string data, PluginTargetMode targetmode)
			=> Send<ResponseVoid>(new Ts3Command("plugincmd") {
				{ "name", name },
				{ "data", data },
				{ "targetmode", (int)targetmode },
			}).OnlyError();

		// Splitted base commands

		public override R<ServerGroupAddResponse, CommandError> ServerGroupAdd(string name, GroupType? type = null)
		{
			var result = SendNotifyCommand(new Ts3Command("servergroupadd") {
				{ "name", name },
				{ "type", (int?)type }
			}, NotificationType.ServerGroupList);
			if (!result.Ok)
				return result.Error;
			return result.Value.Notifications
				.Cast<ServerGroupList>()
				.Where(x => x.Name == name)
				.Take(1)
				.Select(x => new ServerGroupAddResponse() { ServerGroupId = x.ServerGroupId })
				.WrapSingle();
		}

		public override R<FileUpload, CommandError> FileTransferInitUpload(ChannelIdT channelId, string path, string channelPassword, ushort clientTransferId,
			long fileSize, bool overwrite, bool resume)
		{
			var result = SendNotifyCommand(new Ts3Command("ftinitupload") {
				{ "cid", channelId },
				{ "name", path },
				{ "cpw", channelPassword },
				{ "clientftfid", clientTransferId },
				{ "size", fileSize },
				{ "overwrite", overwrite },
				{ "resume", resume }
			}, NotificationType.FileUpload, NotificationType.FileTransferStatus);
			if (!result.Ok)
				return result.Error;
			if (result.Value.NotifyType == NotificationType.FileUpload)
				return result.UnwrapNotification<FileUpload>().WrapSingle();
			else
			{
				var ftresult = result.UnwrapNotification<FileTransferStatus>().WrapSingle();
				if (!ftresult)
					return ftresult.Error;
				return new CommandError() { Id = ftresult.Value.Status, Message = ftresult.Value.Message };
			}
		}

		public override R<FileDownload, CommandError> FileTransferInitDownload(ChannelIdT channelId, string path, string channelPassword, ushort clientTransferId,
			long seek)
		{
			var result = SendNotifyCommand(new Ts3Command("ftinitdownload") {
				{ "cid", channelId },
				{ "name", path },
				{ "cpw", channelPassword },
				{ "clientftfid", clientTransferId },
				{ "seekpos", seek } }, NotificationType.FileDownload, NotificationType.FileTransferStatus);
			if (!result.Ok)
				return result.Error;
			if (result.Value.NotifyType == NotificationType.FileDownload)
				return result.UnwrapNotification<FileDownload>().WrapSingle();
			else
			{
				var ftresult = result.UnwrapNotification<FileTransferStatus>().WrapSingle();
				if (!ftresult)
					return ftresult.Error;
				return new CommandError() { Id = ftresult.Value.Status, Message = ftresult.Value.Message };
			}
		}

		#endregion

		private enum Ts3ClientStatus
		{
			Disconnected,
			Disconnecting,
			Connected,
			Connecting,
		}
	}

	internal class ConnectionContext
	{
		public bool WasExit { get; set; }
		public Reason? ExitReason { get; set; }
	}
}
