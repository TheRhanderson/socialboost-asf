using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SocialBoost;
internal sealed class DbHelper {

	private const string FilePath = "plugins/socialboost-db.json";

	public static bool VerificarEnvioItem(string botName, string boostType, string idToCheck) {

		string jsonContent = File.ReadAllText(FilePath);
		Dictionary<string, BotData> data = JsonConvert.DeserializeObject<Dictionary<string, BotData>>(jsonContent)
										  ?? [];

		// Verifica se o bot já existe no arquivo JSON, se não, adiciona
		if (!data.TryGetValue(botName, out BotData? botData)) {
			botData = new BotData();
			data[botName] = botData;
		}

		List<string> reviewList = GetReviewList(boostType, botData);
		if (reviewList.Contains(idToCheck)) {
			return true; // Já existe, retorna true
		}

		return false;

	}

	public static void AdicionarEnvioItem(string botName, string boostType, string idToCheck) {

		string jsonContent = File.ReadAllText(FilePath);
		Dictionary<string, BotData> data = JsonConvert.DeserializeObject<Dictionary<string, BotData>>(jsonContent)
										  ?? [];

		if (!data.TryGetValue(botName, out BotData? botData)) {
			botData = new BotData();
			data[botName] = botData;
		}

		List<string> reviewList = GetReviewList(boostType, botData);
		reviewList.Add(idToCheck);
		File.WriteAllText(FilePath, JsonConvert.SerializeObject(data, Formatting.Indented));

	}

	private static List<string> GetReviewList(string boostType, BotData botData) =>
		boostType switch {
			"Reviews" => botData.Reviews,
			"SharedLike" => botData.SharedLike,
			"SharedFav" => botData.SharedFav,
			_ => throw new ArgumentException("Tipo de revisão inválido"),
		};

	public sealed class BotData {
		public List<string> Reviews { get; set; } = [];
		public List<string> SharedLike { get; set; } = [];
		public List<string> SharedFav { get; set; } = [];
	}
}
