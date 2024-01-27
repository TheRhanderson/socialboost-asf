using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

	public static async Task<bool> AdicionarEnvioItem(string botName, string boostType, string idToCheck) {

		string filePath = FilePath;
		string jsonContent = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
		Dictionary<string, BotData> data = JsonConvert.DeserializeObject<Dictionary<string, BotData>>(jsonContent)
										  ?? [];

		// Verifica se o bot já existe no dicionário, se não, adiciona
		if (!data.TryGetValue(botName, out BotData? botData)) {
			botData = new BotData();
			data[botName] = botData;
		}

		List<string> reviewList = GetReviewList(boostType, botData);
		if (reviewList.Contains(idToCheck)) {
			return false;
		}

		reviewList.Add(idToCheck);
		await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(data, Formatting.Indented)).ConfigureAwait(false);

		return true;
	}

	public static async Task<bool> RemoverItem(string botName, string boostType, string idToRemove) {

		string filePath = FilePath;
		string jsonContent = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
		Dictionary<string, BotData> data = JsonConvert.DeserializeObject<Dictionary<string, BotData>>(jsonContent)
										  ?? [];

		// Verifica se o bot existe no dicionário
		if (data.TryGetValue(botName, out BotData? botData)) {
			// Obtém a lista correspondente com base no tipo de revisão
			List<string> reviewList = GetReviewList(boostType, botData);

			if (reviewList.Remove(idToRemove)) {
				await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(data, Formatting.Indented)).ConfigureAwait(false);
				return true;
			}
		}
		return false;
	}

	public static List<string> GetReviewList(string boostType, BotData botData) =>
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
