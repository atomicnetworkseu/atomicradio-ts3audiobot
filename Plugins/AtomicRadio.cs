using System;
using System.Collections.Generic;
using TS3AudioBot;
using TS3AudioBot.Audio;
using TS3AudioBot.Plugins;
using TS3Client.Full;
using TS3Client.Messages;
using TS3AudioBot.CommandSystem;

public class AtomicRadio : IBotPlugin
{

	public Ts3FullClient TS3FullClient { get; set; }
	public Ts3Client Ts3Client { get; set; }
	private static NLog.Logger Log;
	public ulong currentChannel = 0;
	public Dictionary<ushort, Boolean> listeners;


	public AtomicRadio(PlayManager playManager, Ts3Client ts3Client)
	{
		Ts3Client = ts3Client;
		Log = NLog.LogManager.GetLogger($"TS3AudioBot.Plugins.AtomicRadio"); ;
		listeners = new Dictionary<ushort, Boolean>();
	}

	public void Initialize()
	{
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

	private void OnEachClientLeftView(object sender, ClientLeftView e)
	{
		Ts3FullClient senderClient = (Ts3FullClient)sender;
		if (senderClient.ClientId == e.ClientId)
		{
			listeners.Clear();
			return;
		}

		var client = Ts3Client.GetCachedClientById(e.ClientId).Value;
		if (client == null)
		{
			return;
		}
		if (client.Uid == null)
		{
			return;
		}

		if (listeners.ContainsKey(e.ClientId))
		{
			listeners.Remove(e.ClientId);
			//Log.Info(listeners.Count);
		}
	}

	private void OnEachClientEnterView(object sender, ClientEnterView e)
	{
		Ts3FullClient senderClient = (Ts3FullClient)sender;
		var botClient = Ts3Client.GetCachedClientById(senderClient.ClientId).Value;
		if(currentChannel == 0)
		{
			listeners.Clear();
			currentChannel = e.TargetChannelId;
		}
		if (senderClient.ClientId == e.ClientId)
		{
			listeners.Clear();
			currentChannel = e.TargetChannelId;
			return;
		}

		var client = Ts3Client.GetCachedClientById(e.ClientId).Value;

		if (client == null)
		{
			return;
		}
		if (client.Uid == null)
		{
			return;
		}

		if (currentChannel != e.TargetChannelId)
		{
			if (listeners.ContainsKey(e.ClientId))
			{
				listeners.Remove(e.ClientId);
				//Log.Info(listeners.Count);
				return;
			}
		}

		if (!listeners.ContainsKey(e.ClientId))
		{
			listeners.Add(e.ClientId, true);
		}
		//Log.Info(listeners.Count);
	}

	private void OnEachClientMoved(object sender, ClientMoved e)
	{
		Ts3FullClient senderClient = (Ts3FullClient)sender;
		var botClient = Ts3Client.GetCachedClientById(senderClient.ClientId).Value;
		if (currentChannel == 0)
		{
			listeners.Clear();
			currentChannel = e.TargetChannelId;
		}
		if (senderClient.ClientId == e.ClientId)
		{
			listeners.Clear();
			currentChannel = e.TargetChannelId;
			return;
		}

		if (currentChannel != e.TargetChannelId)
		{
			if(listeners.ContainsKey(e.ClientId))
			{
				listeners.Remove(e.ClientId);
				//Log.Info(listeners.Count);
				return;
			}
		}

		if (!listeners.ContainsKey(e.ClientId))
		{
			listeners.Add(e.ClientId, true);
		}
		//Log.Info(listeners.Count);
	}

	[Command("listeners")]
	public string ListenerCommand()
	{
		if(listeners == null)
		{
			return "0";
		}
		Log.Info(currentChannel);
		return "" + listeners.Count;
	}

}
