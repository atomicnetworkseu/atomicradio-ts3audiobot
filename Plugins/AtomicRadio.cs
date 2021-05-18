using System.Net.Http;
using System.Collections.Generic;
using TS3AudioBot;
using TS3AudioBot.Plugins;
using TS3Client.Full;
using TS3Client.Messages;

public class atomicradio : IBotPlugin
{

	public Ts3FullClient TS3FullClient { get; set; }
	private static NLog.Logger Log;
	public Dictionary<ushort, ulong> currentChannels;
	public Dictionary<ushort, List<ushort>> listeners;
	private HttpClient client = new HttpClient();

	public atomicradio()
	{
		Log = NLog.LogManager.GetLogger($"TS3AudioBot.Plugins.atomicradio"); ;
		listeners = new Dictionary<ushort, List<ushort>>();
		currentChannels = new Dictionary<ushort, ulong>();
	}

	public void Initialize()
	{
		updateChannel();
		TS3FullClient.OnEachClientMoved += OnEachClientMoved;
		TS3FullClient.OnEachClientEnterView += OnEachClientEnterView;
		TS3FullClient.OnEachClientLeftView += OnEachClientLeftView;
		Log.Info("atomicradio plugin v1.0.0 by Kacper Mura loaded.");
	}

	public void Dispose()
	{
		TS3FullClient.OnEachClientMoved -= OnEachClientMoved;
		TS3FullClient.OnEachClientEnterView -= OnEachClientEnterView;
		TS3FullClient.OnEachClientLeftView -= OnEachClientLeftView;
	}

	private void updateChannel(ushort botId = 0, ulong channelId = 0)
	{
		if(channelId == 0)
		{
			channelId = TS3FullClient.WhoAmI().Value.ChannelId;
		}
		if (botId == 0)
		{
			botId = TS3FullClient.WhoAmI().Value.ClientId;
		}

		ulong botChannel;
		if(!currentChannels.TryGetValue(botId, out botChannel))
		{
			currentChannels.Add(botId, channelId);
		} else
		{
			currentChannels[botId] = channelId;
		}

		List<ushort> botListeners;
		if(!listeners.TryGetValue(botId, out botListeners))
		{
			listeners.Add(botId, new List<ushort>());
			return;
		}

		botListeners.Clear();
		sendUpdate();
	}

	private void sendUpdate()
	{
		var currentListeners = listeners[TS3FullClient.ClientId];
		var values = new Dictionary<string, string> { { "type", "teamspeak" }, { "botId", "" + TS3FullClient.WhoAmI().Value.ClientId }, { "value", "" + currentListeners.Count } };
		client.PostAsync("https://api.atomicradio.eu/channels/listeners?token=Btz7asgakNLBXFT5n4wu9SemF6hvuT", new FormUrlEncodedContent(values));
	}

	private void OnEachClientLeftView(object sender, ClientLeftView e)
	{
		if (e.ClientId == TS3FullClient.ClientId)
		{
			return;
		}
		ulong currentChannel;
		if(!currentChannels.TryGetValue(TS3FullClient.ClientId, out currentChannel))
		{
			return;
		}

		var currentListeners = listeners[TS3FullClient.ClientId];
		if (e.SourceChannelId == currentChannel)
		{
			currentListeners.Remove(e.ClientId);
			sendUpdate();
			//Log.Info("atomicradio: OnEachClientLeftView: {} {} {}", e.ClientId, e.TargetChannelId, currentListeners.Count);
		}
	}

	private void OnEachClientEnterView(object sender, ClientEnterView e)
	{
		if(e.ClientId == TS3FullClient.ClientId)
		{
			return;
		}
		ulong currentChannel;
		if (!currentChannels.TryGetValue(TS3FullClient.ClientId, out currentChannel))
		{
			return;
		}
		var currentListeners = listeners[TS3FullClient.ClientId];
		if (e.TargetChannelId == currentChannel)
		{
			currentListeners.Add(e.ClientId);
			sendUpdate();
			//Log.Info("atomicradio: OnEachClientEnterView: {} {} {}", e.ClientId, e.TargetChannelId, currentListeners.Count);
		}
	}

	private void OnEachClientMoved(object sender, ClientMoved e)
	{
		if (e.ClientId == TS3FullClient.ClientId)
		{
			updateChannel(e.ClientId, e.TargetChannelId);
			return;
		}
		ulong currentChannel;
		if (!currentChannels.TryGetValue(TS3FullClient.ClientId, out currentChannel))
		{
			return;
		}
		var currentListeners = listeners[TS3FullClient.ClientId];
		var hasClient = currentListeners.Contains(e.ClientId);
		if (e.TargetChannelId == currentChannel)
		{
			if(!hasClient)
			{
				currentListeners.Add(e.ClientId);
				sendUpdate();
			}
		} else if(hasClient)
		{
			currentListeners.Remove(e.ClientId);
			sendUpdate();
		}
		//Log.Info("atomicradio: OnEachClientMoved: {} {} {}", e.ClientId, e.TargetChannelId, currentListeners.Count);
	}

}
