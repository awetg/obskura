using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Database
{
	String dbConnection;
	SQLiteProviderFactory database = new SQLiteProviderFactory();
	IDbConnection connection;

	/// <summary>
	///     Default Constructor for SQLiteDatabase Class.
	/// </summary>
	public Database()
	{
	    connection = database.CreateConnection ();
		connection.ConnectionString = "Data Source=recipes.s3db";
	}

	/// <summary>
	///     Single Param Constructor for specifying the DB file.
	/// </summary>
	/// <param name="inputFile">The File containing the DB</param>
	public Database(String inputFile)
	{
		connection = database.CreateConnection ();
		connection.ConnectionString = String.Format("Data Source={0}", inputFile);
	}

	/// <summary>
	///     Single Param Constructor for specifying advanced connection options.
	/// </summary>
	/// <param name="connectionOpts">A dictionary containing all desired options and their values</param>
	public Database(Dictionary<String, String> connectionOpts)
	{
		String str = "";
		foreach (KeyValuePair<String, String> row in connectionOpts)
		{
			str += String.Format("{0}={1}; ", row.Key, row.Value);
		}
		str = str.Trim().Substring(0, str.Length - 1);
		connection = database.CreateConnection ();
		connection.ConnectionString = str;
	}

	/// <summary>
	///     Allows the programmer to run a query against the Database.
	/// </summary>
	/// <param name="sql">The SQL to run</param>
	/// <returns>A DataTable containing the result set.</returns>
	public DataTable Query(string sql)
	{
		DataTable dt = new DataTable();
		try
		{
			connection.Open();
			IDbCommand com = connection.CreateCommand();
			com.CommandText = sql;
			IDataReader reader = com.ExecuteReader();
			dt.Load(reader);
			reader.Close();
			connection.Close();
		}
		catch (Exception e) {
			throw new Exception (e.Message);
		}

		return dt;
	}



	/// <summary>
	///     Allows the programmer to interact with the database for purposes other than a query.
	/// </summary>
	/// <param name="sql">The SQL to be run.</param>
	/// <returns>An Integer containing the number of rows updated.</returns>
	public int NonQuery(string sql)
	{
		try
		{
			connection.Open();
			IDbCommand com = connection.CreateCommand();
			com.CommandText = sql;
			int rowsUpdated = com.ExecuteNonQuery();
			connection.Close();
			return rowsUpdated;
		}
		catch (Exception e) {
			throw new Exception (e.Message);
		}
	}



	/// <summary>
	///     Allows the programmer to easily update rows in the DB.
	/// </summary>
	/// <param name="tableName">The table to update.</param>
	/// <param name="data">A dictionary containing Column names and their new values.</param>
	/// <param name="where">The where clause for the update statement.</param>
	/// <returns>A boolean true or false to signify success or failure.</returns>
	public bool Update(String tableName, Dictionary<String, String> data, String where)
	{
		String vals = "";
		Boolean returnCode = true;
		if (data.Count >= 1)
		{
			foreach (KeyValuePair<String, String> val in data)
			{
				vals += String.Format(" {0} = '{1}',", val.Key.ToString(), val.Value.ToString());
			}
			vals = vals.Substring(0, vals.Length - 1);
		}
		try
		{
			this.NonQuery(String.Format("update {0} set {1} where {2};", tableName, vals, where));
		}
		catch
		{
			returnCode = false;
		}
		return returnCode;
	}

	/// <summary>
	///     Allows the programmer to easily delete rows from the DB.
	/// </summary>
	/// <param name="tableName">The table from which to delete.</param>
	/// <param name="where">The where clause for the delete.</param>
	/// <returns>A boolean true or false to signify success or failure.</returns>
	public bool Delete(String tableName, String where)
	{
		Boolean returnCode = true;
		try
		{
			this.NonQuery(String.Format("delete from {0} where {1};", tableName, where));
		}
		catch (Exception fail)
		{
			Debug.Log(fail.Message);
			returnCode = false;
		}
		return returnCode;
	}

	/// <summary>
	///     Allows the programmer to easily insert into the DB
	/// </summary>
	/// <param name="tableName">The table into which we insert the data.</param>
	/// <param name="data">A dictionary containing the column names and data for the insert.</param>
	/// <returns>A boolean true or false to signify success or failure.</returns>
	public bool Insert(String tableName, Dictionary<String, String> data)
	{
		String columns = "";
		String values = "";
		Boolean returnCode = true;
		foreach (KeyValuePair<String, String> val in data)
		{
			columns += String.Format(" {0},", val.Key.ToString());
			values += String.Format(" '{0}',", val.Value);
		}
		columns = columns.Substring(0, columns.Length - 1);
		values = values.Substring(0, values.Length - 1);
		try
		{
			this.NonQuery(String.Format("insert into {0}({1}) values({2});", tableName, columns, values));
		}
		catch(Exception fail)
		{
			Debug.Log(fail.Message);
			returnCode = false;
		}
		return returnCode;
	}

	/// <summary>
	///     Allows the programmer to easily delete all data from the DB.
	/// </summary>
	/// <returns>A boolean true or false to signify success or failure.</returns>
	public bool ClearDB()
	{
		DataTable tables;
		try
		{
			tables = this.Query("select NAME from SQLITE_MASTER where type='table' order by NAME;");
			foreach (DataRow table in tables.Rows)
			{
				this.ClearTable(table["NAME"].ToString());
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	///     Allows the user to easily clear all data from a specific table.
	/// </summary>
	/// <param name="table">The name of the table to clear.</param>
	/// <returns>A boolean true or false to signify success or failure.</returns>
	public bool ClearTable(String table)
	{
		try
		{

			this.NonQuery(String.Format("delete from {0};", table));
			return true;
		}
		catch
		{
			return false;
		}
	}       
}

