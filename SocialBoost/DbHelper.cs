using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
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

		// Lê o conteúdo do arquivo JSON de forma assíncrona
		string jsonContent = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

		// Desserializa o JSON para um dicionário
		Dictionary<string, BotData> data = JsonConvert.DeserializeObject<Dictionary<string, BotData>>(jsonContent)
										  ?? [];

		// Verifica se o bot já existe no dicionário, se não, adiciona
		if (!data.TryGetValue(botName, out BotData? botData)) {
			botData = new BotData();
			data[botName] = botData;
		}

		// Obtém a lista correspondente com base no tipo de revisão
		List<string> reviewList = GetReviewList(boostType, botData);

		ASF.ArchiLogger.LogGenericInfo($"{botName} {boostType} {idToCheck}");

		// Verifica se o ID já existe na lista
		if (reviewList.Contains(idToCheck)) {
			ASF.ArchiLogger.LogGenericInfo($"ID já existe na lista: {idToCheck}");
			return false;
		}

		// Adiciona o ID à lista do tipo correto
		reviewList.Add(idToCheck);

		// Serializa e escreve o dicionário de volta ao arquivo de forma assíncrona
		await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(data, Formatting.Indented)).ConfigureAwait(false);

		return true;
	}


	public static async Task<bool> RemoverItem(string botName, string boostType, string idToRemove) {
		string filePath = FilePath;

		// Lê o conteúdo do arquivo JSON de forma assíncrona
		string jsonContent = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

		// Desserializa o JSON para um dicionário
		Dictionary<string, BotData> data = JsonConvert.DeserializeObject<Dictionary<string, BotData>>(jsonContent)
										  ?? [];

		// Verifica se o bot existe no dicionário
		if (data.TryGetValue(botName, out BotData? botData)) {
			// Obtém a lista correspondente com base no tipo de revisão
			List<string> reviewList = GetReviewList(boostType, botData);

			ASF.ArchiLogger.LogGenericInfo($"{botName} {boostType} {idToRemove}");

			// Tenta remover o ID da lista e verifica se foi removido com sucesso
			if (reviewList.Remove(idToRemove)) {
				// Serializa e escreve o dicionário de volta ao arquivo de forma assíncrona
				await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(data, Formatting.Indented)).ConfigureAwait(false);

				ASF.ArchiLogger.LogGenericInfo($"ID removido com sucesso: {idToRemove}");
				return true;
			}
		}

		// Se não entrou no bloco do "if" ou se a remoção não foi bem-sucedida, retorna false
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
