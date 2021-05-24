using System.Net.Http;
using System.Collections.Generic;
using TS3AudioBot;
using TS3AudioBot.Plugins;
using TSLib.Full;
using TSLib.Messages;
using TS3AudioBot.Audio;

public class atomicradio : IBotPlugin
{

	public TsFullClient TS3FullClient { get; set; }
	public Ts3Client ts3Client;
	public PlayManager playManager;
	private static NLog.Logger Log;
	private HttpClient client = new HttpClient();

	public atomicradio(PlayManager playManager, Ts3Client ts3Client)
	{
		this.ts3Client = ts3Client;
		this.playManager = playManager;
		Log = NLog.LogManager.GetLogger($"TS3AudioBot.Plugins.atomicradio");
	}

	public void Initialize()
	{
		TS3FullClient.OnEachClientMoved += OnEachClientMoved;
		TS3FullClient.OnEachClientEnterView += OnEachClientEnterView;
		TS3FullClient.OnEachClientLeftView += OnEachClientLeftView;
		Log.Info("atomicradio plugin v2.0.0 by Kacper Mura loaded.");
	}

	public void Dispose()
	{
		TS3FullClient.OnEachClientMoved -= OnEachClientMoved;
		TS3FullClient.OnEachClientEnterView -= OnEachClientEnterView;
		TS3FullClient.OnEachClientLeftView -= OnEachClientLeftView;
	}

	private void sendUpdate(int listeners)
	{
		var self = TS3FullClient.Book.OwnClient;
		if (self is null)
		{
			return;
		}
		var values = new Dictionary<string, string> { { "type", "teamspeak" }, { "botId", "" + self.Id.Value }, { "value", "" + listeners }, { "station", getStation() } };
		client.PostAsync("https://api.atomicradio.eu/channels/listeners?token=Btz7asgakNLBXFT5n4wu9SemF6hvuT", new FormUrlEncodedContent(values));
	}

	private void OnEachClientLeftView(object sender, ClientLeftView e)
	{
		var self = TS3FullClient.Book.OwnClient;
		if (self is null)
		{
			return;
		}
		var ownChannel = self.Channel;
		if (e.TargetChannelId.Value == ownChannel.Value)
		{
			sendUpdate(ts3Client.ownChannelClients.Length);
			Log.Info("atomicradio: OnEachClientMoved: {} {}", ts3Client.ownChannelClients.Length, getStation());
		}
	}

	private void OnEachClientEnterView(object sender, ClientEnterView e)
	{
		var self = TS3FullClient.Book.OwnClient;
		if (self is null)
		{
			return;
		}
		var ownChannel = self.Channel;
		if (e.TargetChannelId.Value == ownChannel.Value)
		{
			sendUpdate(ts3Client.ownChannelClients.Length);
			Log.Info("atomicradio: OnEachClientMoved: {} {}", ts3Client.ownChannelClients.Length, getStation());
		}
	}

	private void OnEachClientMoved(object sender, ClientMoved e)
	{
		var self = TS3FullClient.Book.OwnClient;
		if (self is null)
		{
			return;
		}
		var ownChannel = self.Channel;
		if (e.TargetChannelId.Value == ownChannel.Value)
		{
			sendUpdate(ts3Client.ownChannelClients.Length);
			Log.Info("atomicradio: OnEachClientMoved: {} {}", ts3Client.ownChannelClients.Length, getStation());
		}
	}

	private string getStation()
	{
		var player = playManager.CurrentPlayData;
		if (player is null)
		{
			return null;
		}
		var sourceLink = player.SourceLink;
		var station = sourceLink.Split(".eu/")[1].Split("/")[0];
		if (station is null)
		{
			return null;
		}
		return station.ToUpper();
	}

}
