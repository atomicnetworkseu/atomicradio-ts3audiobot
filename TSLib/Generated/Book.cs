// TSLib - A free TeamSpeak 3 and 5 client library
// Copyright (C) 2017  TSLib contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the Open Software License v. 3.0
//
// You should have received a copy of the Open Software License along with this
// program. If not, see <https://opensource.org/licenses/OSL-3.0>.
// <auto-generated />













using System.Collections.Generic;

#pragma warning disable CS8019 // Ignore unused imports
using i8  = System.SByte;
using u8  = System.Byte;
using i16 = System.Int16;
using u16 = System.UInt16;
using i32 = System.Int32;
using u32 = System.UInt32;
using i64 = System.Int64;
using u64 = System.UInt64;
using f32 = System.Single;
using f64 = System.Double;
using str = System.String;

using DateTime = System.DateTime;
using Duration = System.TimeSpan;
using DurationSeconds = System.TimeSpan;
using DurationMilliseconds = System.TimeSpan;
using SocketAddr = System.String;
using IpAddr = System.String;
using Ts3ErrorCode = TSLib.TsErrorCode;
using Ts3Permission = TSLib.TsPermission;

using IconHash = System.Int32;
using ConnectionId = System.UInt32;
using EccKeyPubP256 = TSLib.Uid;
#pragma warning restore CS8019

#nullable enable
namespace TSLib.Full.Book
{

	public sealed partial class ServerGroup
	{
		#pragma warning disable CS8618
		public ServerGroup()
		#pragma warning restore CS8618
		{
			
		}

	
		public GroupType GroupType { get; set; }
		public IconHash IconId { get; set; }
		public ServerGroupId Id { get; internal set; }
		public bool IsPermanent { get; set; }
		public str Name { get; set; }
		public GroupNamingMode NamingMode { get; set; }
		public i32 NeededMemberAddPower { get; set; }
		public i32? NeededMemberRemovePower { get; set; }
		public i32 NeededModifyPower { get; set; }
		public i32 SortId { get; set; }
	}

	public sealed partial class File
	{
		#pragma warning disable CS8618
		public File()
		#pragma warning restore CS8618
		{
			
		}

	
		public bool IsFile { get; set; }
		public DateTime LastChanged { get; set; }
		public str Path { get; set; }
		public i64 Size { get; set; }
	}

	public sealed partial class OptionalChannelData
	{
		#pragma warning disable CS8618
		public OptionalChannelData()
		#pragma warning restore CS8618
		{
			
		}

	
		public str Description { get; set; }
	}

	public sealed partial class Channel
	{
		#pragma warning disable CS8618
		public Channel()
		#pragma warning restore CS8618
		{
			
		}

	
		public ChannelType ChannelType { get; set; }
		public Codec? Codec { get; set; }
		public i32? CodecLatencyFactor { get; set; }
		public u8? CodecQuality { get; set; }
		public Duration? DeleteDelay { get; set; }
		public bool ForcedSilence { get; internal set; }
		public bool? HasPassword { get; set; }
		public IconHash? IconId { get; set; }
		public ChannelId Id { get; internal set; }
		public bool? IsDefault { get; set; }
		public bool? IsPrivate { get; set; }
		public bool? IsUnencrypted { get; set; }
		public MaxClients? MaxClients { get; set; }
		public MaxClients? MaxFamilyClients { get; set; }
		public str Name { get; set; }
		public i32? NeededTalkPower { get; set; }
		public OptionalChannelData? OptionalData { get; internal set; }
		public ChannelId Order { get; set; }
		public ChannelId Parent { get; set; }
		public ChannelPermissionHint? PermissionHints { get; set; }
		public str? PhoneticName { get; set; }
		public bool Subscribed { get; set; }
		public str? Topic { get; set; }
	}

	public sealed partial class OptionalClientData
	{
		#pragma warning disable CS8618
		public OptionalClientData()
		#pragma warning restore CS8618
		{
			
		}

	
		public DateTime Created { get; internal set; }
		public DateTime LastConnected { get; internal set; }
		public str? LoginName { get; internal set; }
		public u64 MonthBytesDownloaded { get; internal set; }
		public u64 MonthBytesUploaded { get; internal set; }
		public str Platform { get; set; }
		public u64 TotalBytesDownloaded { get; internal set; }
		public u64 TotalBytesUploaded { get; internal set; }
		public u32 TotalConnection { get; internal set; }
		public str Version { get; set; }
		public str VersionSign { get; set; }
	}

	public sealed partial class ConnectionClientData
	{
		#pragma warning disable CS8618
		public ConnectionClientData()
		#pragma warning restore CS8618
		{
			
		}

	
		public u64 BandwidthReceivedLastMinuteControl { get; internal set; }
		public u64 BandwidthReceivedLastMinuteKeepalive { get; internal set; }
		public u64 BandwidthReceivedLastMinuteSpeech { get; internal set; }
		public u64 BandwidthReceivedLastSecondControl { get; internal set; }
		public u64 BandwidthReceivedLastSecondKeepalive { get; internal set; }
		public u64 BandwidthReceivedLastSecondSpeech { get; internal set; }
		public u64 BandwidthSentLastMinuteControl { get; internal set; }
		public u64 BandwidthSentLastMinuteKeepalive { get; internal set; }
		public u64 BandwidthSentLastMinuteSpeech { get; internal set; }
		public u64 BandwidthSentLastSecondControl { get; internal set; }
		public u64 BandwidthSentLastSecondKeepalive { get; internal set; }
		public u64 BandwidthSentLastSecondSpeech { get; internal set; }
		public u64 BytesReceivedControl { get; internal set; }
		public u64 BytesReceivedKeepalive { get; internal set; }
		public u64 BytesReceivedSpeech { get; internal set; }
		public u64 BytesSentControl { get; internal set; }
		public u64 BytesSentKeepalive { get; internal set; }
		public u64 BytesSentSpeech { get; internal set; }
		public SocketAddr? ClientAddress { get; internal set; }
		public f32 ClientToServerPacketlossControl { get; internal set; }
		public f32 ClientToServerPacketlossKeepalive { get; internal set; }
		public f32 ClientToServerPacketlossSpeech { get; internal set; }
		public f32 ClientToServerPacketlossTotal { get; internal set; }
		public Duration ConnectedTime { get; internal set; }
		public u64 FiletransferBandwidthReceived { get; internal set; }
		public u64 FiletransferBandwidthSent { get; internal set; }
		public Duration IdleTime { get; internal set; }
		public u64 PacketsReceivedControl { get; internal set; }
		public u64 PacketsReceivedKeepalive { get; internal set; }
		public u64 PacketsReceivedSpeech { get; internal set; }
		public u64 PacketsSentControl { get; internal set; }
		public u64 PacketsSentKeepalive { get; internal set; }
		public u64 PacketsSentSpeech { get; internal set; }
		public Duration Ping { get; internal set; }
		public Duration PingDeviation { get; internal set; }
		public f32 ServerToClientPacketlossControl { get; internal set; }
		public f32 ServerToClientPacketlossKeepalive { get; internal set; }
		public f32 ServerToClientPacketlossSpeech { get; internal set; }
		public f32 ServerToClientPacketlossTotal { get; internal set; }
	}

	public sealed partial class Client
	{
		#pragma warning disable CS8618
		public Client()
		#pragma warning restore CS8618
		{
			ServerGroups = new HashSet<ServerGroupId>();
			
		}

	
		public str AvatarHash { get; internal set; }
		public str? AwayMessage { get; set; }
		public str Badges { get; set; }
		public ChannelId Channel { get; set; }
		public ChannelGroupId ChannelGroup { get; set; }
		public ClientType ClientType { get; internal set; }
		public ConnectionClientData? ConnectionData { get; internal set; }
		public str CountryCode { get; internal set; }
		public ClientDbId DatabaseId { get; internal set; }
		public str Description { get; set; }
		public IconHash IconId { get; internal set; }
		public ClientId Id { get; internal set; }
		public ChannelId InheritedChannelGroupFromChannel { get; internal set; }
		public bool InputHardwareEnabled { get; set; }
		public bool InputMuted { get; set; }
		public bool IsChannelCommander { get; set; }
		public bool IsPrioritySpeaker { get; set; }
		public bool IsRecording { get; set; }
		public str Metadata { get; set; }
		public str Name { get; set; }
		public i32 NeededServerqueryViewPower { get; internal set; }
		public OptionalClientData? OptionalData { get; internal set; }
		public bool OutputHardwareEnabled { get; set; }
		public bool OutputMuted { get; set; }
		public bool OutputOnlyMuted { get; set; }
		public ClientPermissionHint? PermissionHints { get; set; }
		public str PhoneticName { get; set; }
		public HashSet<ServerGroupId> ServerGroups { get; set; }
		public i32 TalkPower { get; internal set; }
		public bool TalkPowerGranted { get; set; }
		public TalkPowerRequest? TalkPowerRequest { get; internal set; }
		public Uid? Uid { get; internal set; }
		public u32 UnreadMessages { get; internal set; }
	}

	public sealed partial class OptionalServerData
	{
		#pragma warning disable CS8618
		public OptionalServerData()
		#pragma warning restore CS8618
		{
			
		}

	
		public u32 AntifloodPointsNeededCommandBlock { get; set; }
		public u32 AntifloodPointsTickReduce { get; set; }
		public bool Autostart { get; set; }
		public u64 ChannelCount { get; internal set; }
		public u16 ClientCount { get; internal set; }
		public u32 ComplainAutobanCount { get; set; }
		public Duration ComplainAutobanTime { get; set; }
		public Duration ComplainRemoveTime { get; set; }
		public u32 ConnectionCount { get; internal set; }
		public ChannelGroupId DefaultChannelAdminGroup { get; set; }
		public u64 DownloadQuota { get; set; }
		public bool HasPassword { get; internal set; }
		public bool LogChannel { get; set; }
		public bool LogClient { get; set; }
		public bool LogFiletransfer { get; set; }
		public bool LogPermissions { get; set; }
		public bool LogQuery { get; set; }
		public bool LogServer { get; set; }
		public str MachineId { get; internal set; }
		public u64 MaxDownloadTotalBandwith { get; set; }
		public u64 MaxUploadTotalBandwith { get; set; }
		public DateTime MinAndroidVersion { get; internal set; }
		public u16 MinClientsForceSilence { get; set; }
		public DateTime MinClientVersion { get; internal set; }
		public DateTime MinIosVersion { get; internal set; }
		public u64 MonthBytesDownloaded { get; internal set; }
		public u64 MonthBytesUploaded { get; internal set; }
		public u8 NeededIdentitySecurityLevel { get; set; }
		public u16 Port { get; internal set; }
		public u32 QueryCount { get; internal set; }
		public u32 QueryOnlineCount { get; internal set; }
		public u16 ReservedSlots { get; set; }
		public u64 TotalBytesDownloaded { get; internal set; }
		public u64 TotalBytesUploaded { get; internal set; }
		public f32 TotalPacketlossControl { get; internal set; }
		public f32 TotalPacketlossKeepalive { get; internal set; }
		public f32 TotalPacketlossSpeech { get; internal set; }
		public f32 TotalPacketlossTotal { get; internal set; }
		public Duration TotalPing { get; internal set; }
		public u64 UploadQuota { get; set; }
		public Duration Uptime { get; internal set; }
		public bool WeblistEnabled { get; set; }
	}

	public sealed partial class ConnectionServerData
	{
		#pragma warning disable CS8618
		public ConnectionServerData()
		#pragma warning restore CS8618
		{
			
		}

	
		public u64 BandwidthReceivedLastMinuteTotal { get; internal set; }
		public u64 BandwidthReceivedLastSecondTotal { get; internal set; }
		public u64 BandwidthSentLastMinuteTotal { get; internal set; }
		public u64 BandwidthSentLastSecondTotal { get; internal set; }
		public u64 BytesReceivedTotal { get; internal set; }
		public u64 BytesSentTotal { get; internal set; }
		public Duration ConnectedTime { get; internal set; }
		public u64 FiletransferBandwidthReceived { get; internal set; }
		public u64 FiletransferBandwidthSent { get; internal set; }
		public u64 FiletransferBytesReceivedTotal { get; internal set; }
		public u64 FiletransferBytesSentTotal { get; internal set; }
		public f32 PacketlossTotal { get; internal set; }
		public u64 PacketsReceivedTotal { get; internal set; }
		public u64 PacketsSentTotal { get; internal set; }
		public Duration Ping { get; internal set; }
	}

	public sealed partial class Server
	{
		#pragma warning disable CS8618
		public Server()
		#pragma warning restore CS8618
		{
			Ips = new HashSet<IpAddr>();
			
		}

	
		public bool AskForPrivilegekey { get; internal set; }
		public CodecEncryptionMode CodecEncryptionMode { get; set; }
		public ConnectionServerData? ConnectionData { get; internal set; }
		public DateTime Created { get; internal set; }
		public ChannelGroupId DefaultChannelGroup { get; set; }
		public ServerGroupId DefaultServerGroup { get; set; }
		public Duration HostbannerGfxInterval { get; set; }
		public str HostbannerGfxUrl { get; set; }
		public HostBannerMode HostbannerMode { get; set; }
		public str HostbannerUrl { get; set; }
		public str HostbuttonGfxUrl { get; set; }
		public str HostbuttonTooltip { get; set; }
		public str HostbuttonUrl { get; set; }
		public str Hostmessage { get; set; }
		public HostMessageMode HostmessageMode { get; set; }
		public IconHash IconId { get; internal set; }
		public HashSet<IpAddr> Ips { get; internal set; }
		public LicenseType License { get; internal set; }
		public u16 MaxClients { get; internal set; }
		public str Name { get; internal set; }
		public str Nickname { get; internal set; }
		public OptionalServerData? OptionalData { get; internal set; }
		public str PhoneticName { get; set; }
		public str Platform { get; internal set; }
		public f32 PrioritySpeakerDimmModificator { get; set; }
		public u16 ProtocolVersion { get; internal set; }
		public EccKeyPubP256 PublicKey { get; internal set; }
		public Duration TempChannelDefaultDeleteDelay { get; set; }
		public str Version { get; internal set; }
		public u64 VirtualServerId { get; internal set; }
		public str WelcomeMessage { get; internal set; }
	}

	public sealed partial class Connection
	{
		#pragma warning disable CS8618
		public Connection()
		#pragma warning restore CS8618
		{
			Clients = new Dictionary<ClientId,Client>();
			Channels = new Dictionary<ChannelId,Channel>();
			Groups = new Dictionary<ServerGroupId,ServerGroup>();
			
		}

	
		public Dictionary<ChannelId,Channel> Channels { get; internal set; }
		public Dictionary<ClientId,Client> Clients { get; internal set; }
		public Dictionary<ServerGroupId,ServerGroup> Groups { get; internal set; }
		public ClientId OwnClientId { get; internal set; }
		public Server Server { get; internal set; }
	}

}