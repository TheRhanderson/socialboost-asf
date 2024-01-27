using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SocialBoost;
internal class DbHelper {

	private const string FilePath = "plugins/socialboost-db.json";

	public static bool VerificarEnvioItem(string botName, string reviewsType, string idToCheck) {

		string jsonContent = File.ReadAllText(FilePath);
		Dictionary<string, BotData> data = JsonConvert.DeserializeObject<Dictionary<string, BotData>>(jsonContent)
										  ?? [];

		// Verifica se o bot já existe no arquivo JSON, se não, adiciona
		if (!data.TryGetValue(botName, out BotData? botData)) {
			botData = new BotData();
			data[botName] = botData;
		}

		List<string> reviewList = GetReviewList(reviewsType, botData);
		if (reviewList.Contains(idToCheck)) {
			return false; // Já existe, retorna false
		}

		reviewList.Add(idToCheck);
		File.WriteAllText(FilePath, JsonConvert.SerializeObject(data, Formatting.Indented));
		return true; // Sucesso, retorna true
	}

	private static List<string> GetReviewList(string reviewsType, BotData botData) =>
		reviewsType switch {
			"Reviews" => botData.Reviews,
			"SharedLike" => botData.SharedLike,
			"SharedFav" => botData.SharedFav,
			_ => throw new ArgumentException("Tipo de revisão inválido"),
		};

	public class BotData {
		public List<string> Reviews { get; set; } = [];
		public List<string> SharedLike { get; set; } = [];
		public List<string> SharedFav { get; set; } = [];
	}
}
