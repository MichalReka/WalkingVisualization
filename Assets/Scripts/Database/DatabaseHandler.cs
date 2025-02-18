﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using Newtonsoft.Json;

public class DatabaseHandler : MonoBehaviour
{
    // Start is called before the first frame update
    static string conn;
    static string sqlQuery;
    static IDbConnection dbconn;
    static IDbCommand dbcmd;
    public static string jsonPath = "tempData.json";
    static string DATABASE_NAME = "database.db";
    static string TABLE_NAME = "visualizations_data";
    private static void OpenConnection()
    {

        conn = "URI=file:/" + DATABASE_NAME;
        dbconn = new SqliteConnection("Data Source=" + DATABASE_NAME + ";Version=3;");
        dbcmd = dbconn.CreateCommand();
        dbconn.Open();
    }
    public static void AddDataToTable()
    {
        CheckIfTableCreated();
        OpenConnection();
        string json = System.IO.File.ReadAllText(jsonPath);
        var data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
        string query = "INSERT OR REPLACE INTO " + TABLE_NAME
                + " (animal_prefab_name,population_size,population_part_size,mutation_rate,current_gen,best_distance,best_fitness"
                + ",curr_best_fitness,starting_position,speed,time_below_average_penalty,best_distances,best_fitnesses,best_individual_data,seconds_passed) VALUES ("
                + "'" + data["animalPrefabName"] + "',"
                + data["populationSize"].ToString().Replace(",", ".") + ',' + data["populationPartSize"].ToString().Replace(",", ".") + ',' + data["mutationRate"].ToString().Replace(",", ".") + ',' + data["currentGen"].ToString().Replace(",", ".") + ','
                + data["bestDistance"].ToString().Replace(",", ".") + ',' + data["bestFitness"].ToString().Replace(",", ".") + ',' + data["currBestFitness"].ToString().Replace(",", ".") + ',' + data["startingPosition"].ToString().Replace(",", ".") + ','
                + data["speed"].ToString().Replace(",", ".") + ',' + data["timeBelowAveragePenalty"].ToString().Replace(",", ".") + ",'" + string.Join(", ", data["bestDistances"]) + "','" + string.Join(", ", data["bestFitnesses"])
                + "','" + data["bestAnimalDataJson"] + "','" + data["secondsPassed"] + "');";
        dbcmd.CommandText = query;
        dbcmd.ExecuteNonQuery();
        dbconn.Close();
    }
    public static List<List<string>> ReturnDataFromTable()
    {
        CheckIfTableCreated();
        OpenConnection();
        List<List<string>> tableData = new List<List<string>>();
        IDataReader reader;
        string query = "SELECT animal_prefab_name, population_size,best_distance,current_gen,seconds_passed, id FROM " + TABLE_NAME;
        dbcmd.CommandText = query;
        reader = dbcmd.ExecuteReader();
        while (reader.Read())
        {
            List<string> tempList = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader[i].GetType() == typeof(System.Double))
                {
                    double doubleValue = (double)reader[i];
                    doubleValue = System.Math.Round(doubleValue, 2, System.MidpointRounding.AwayFromZero);
                    tempList.Add(doubleValue.ToString());
                }
                else
                {
                    tempList.Add(reader[i].ToString());
                }
            }
            tableData.Add(tempList);
        }
        dbconn.Close();
        return tableData;
    }
    public static List<string> ReturnAnimalDataJsonArray(string animalPrefabName)
    {
        CheckIfTableCreated();
        OpenConnection();
        List<string> tableData = new List<string>();
        IDataReader reader;
        string query = "SELECT best_individual_data FROM " + TABLE_NAME + " WHERE animal_prefab_name='" + animalPrefabName + "'";
        dbcmd.CommandText = query;
        reader = dbcmd.ExecuteReader();
        while (reader.Read())
        {
            string animalDataJson = "";
            for (int i = 0; i < reader.FieldCount; i++)
            {
                animalDataJson = reader[i].ToString();
            }
            tableData.Add(animalDataJson);
        }
        dbconn.Close();
        return tableData;
    }
    public static string ReturnAnimalDataJsonSingle(string animalId)
    {
        OpenConnection();
        List<string> tableData = new List<string>();
        IDataReader reader;
        string query = "SELECT best_individual_data FROM " + TABLE_NAME + " WHERE id='" + animalId + "'";
        dbcmd.CommandText = query;
        reader = dbcmd.ExecuteReader();
        string animalDataJson = "";
        while (reader.Read())
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                animalDataJson = reader[i].ToString();
            }
        }
        dbconn.Close();
        return animalDataJson;
    }
    private static void CheckIfTableCreated()
    {
        OpenConnection();
        sqlQuery = "CREATE TABLE IF NOT EXISTS [" + TABLE_NAME + "] (" +
            "[id] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT," +
            "[animal_prefab_name] VARCHAR(255)  NOT NULL," +
            "[population_size] INTEGER NOT NULL," +
            "[population_part_size] INTEGER NOT NULL," +
            "[mutation_rate] FLOAT NOT NULL," +
            "[current_gen] INTEGER NOT NULL," +
            "[best_distance] FLOAT NOT NULL," +
            "[best_fitness] FLOAT NOT NULL," +
            "[curr_best_fitness] FLOAT NOT NULL," +
            "[starting_position] FLOAT NOT NULL," +
            "[speed] FLOAT NOT NULL," +
            "[time_below_average_penalty] FLOAT NOT NULL," +
            "[best_distances] TEXT NOT NULL," +
            "[best_fitnesses] TEXT NOT NULL," +
            "[best_individual_data] TEXT NOT NULL," +
            "[seconds_passed] FLOAT NOT NULL)";
        dbcmd.CommandText = sqlQuery;
        dbcmd.ExecuteScalar();
        dbconn.Close();
    }
}
