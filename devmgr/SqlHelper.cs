// ===============================================================================
// Microsoft Data Access Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/daab-rm.asp
//
// SQLHelper.cs
//
// This file contains the implementations of the SqlHelper and SqlHelperParameterCache
// classes.
//
// For more information see the Data Access Application Block Implementation Overview. 
// ===============================================================================
// Release history
// VERSION	DESCRIPTION
//   2.0	Added support for FillDataset, UpdateDataset and "Param" helper methods
//
// ===============================================================================
// Copyright (C) 2000-2001 Microsoft Corporation
// All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
// ==============================================================================

using System;
using System.Data;
using System.Xml;
using System.Data.SqlClient;
using System.Collections;
using System.Configuration;
using System.Text.RegularExpressions;

/// <summary>
/// The SqlHelper class is intended to encapsulate high performance, scalable best practices for 
/// common uses of SqlClient
/// </summary>
public sealed class SqlHelper
{
    public static string _ConnectionString = ConfigurationManager.ConnectionStrings["connStr"].ConnectionString;

    #region private utility methods & constructors

    // Since this class provides only static methods, make the default constructor private to prevent 
    // instances from being created with "new SqlHelper()"
    private SqlHelper() { }

    /// <summary>
    /// This method is used to attach array of SqlParameters to a SqlCommand.
    /// 
    /// This method will assign a value of DbNull to any parameter with a direction of
    /// InputOutput and a value of null.  
    /// 
    /// This behavior will prevent default values from being used, but
    /// this will be the less common case than an intended pure output parameter (derived as InputOutput)
    /// where the user provided no input value.
    /// </summary>
    /// <param name="command">The command to which the parameters will be added</param>
    /// <param name="commandParameters">An array of SqlParameters to be added to command</param>
    private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
    {
        if (command == null) throw new ArgumentNullException("command");
        if (commandParameters != null)
        {
            foreach (SqlParameter p in commandParameters)
            {
                if (p != null)
                {
                    // Check for derived output value with no value assigned
                    if ((p.Direction == ParameterDirection.InputOutput ||
                        p.Direction == ParameterDirection.Input) &&
                        (p.Value == null))
                    {
                        p.Value = DBNull.Value;
                    }
                    command.Parameters.Add(p);
                }
            }
        }
    }

    /// <summary>
    /// This method assigns dataRow column values to an array of SqlParameters
    /// </summary>
    /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
    private static void AssignParameterValues(SqlParameter[] commandParameters, DataRow dataRow)
    {
        if ((commandParameters == null) || (dataRow == null))
        {
            // Do nothing if we get no data
            return;
        }

        int i = 0;
        // Set the parameters values
        foreach (SqlParameter commandParameter in commandParameters)
        {
            // Check the parameter name
            if (commandParameter.ParameterName == null ||
                commandParameter.ParameterName.Length <= 1)
                throw new Exception(
                    string.Format(
                        "Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.",
                        i, commandParameter.ParameterName));
            if (dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) != -1)
                commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
            i++;
        }
    }

    /// <summary>
    /// This method assigns an array of values to an array of SqlParameters
    /// </summary>
    /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
    /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
    private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
    {
        if ((commandParameters == null) || (parameterValues == null))
        {
            // Do nothing if we get no data
            return;
        }

        // We must have the same number of values as we pave parameters to put them in
        if (commandParameters.Length != parameterValues.Length)
        {
            throw new ArgumentException("Parameter count does not match Parameter Value count.");
        }

        // Iterate through the SqlParameters, assigning the values from the corresponding position in the 
        // value array
        for (int i = 0, j = commandParameters.Length; i < j; i++)
        {
            // If the current array value derives from IDbDataParameter, then assign its Value property
            if (parameterValues[i] is IDbDataParameter)
            {
                IDbDataParameter paramInstance = (IDbDataParameter)parameterValues[i];
                if (paramInstance.Value == null)
                {
                    commandParameters[i].Value = DBNull.Value;
                }
                else
                {
                    commandParameters[i].Value = paramInstance.Value;
                }
            }
            else if (parameterValues[i] == null)
            {
                commandParameters[i].Value = DBNull.Value;
            }
            else
            {
                commandParameters[i].Value = parameterValues[i];
            }
        }
    }

    /// <summary>
    /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
    /// to the provided command
    /// </summary>
    /// <param name="command">The SqlCommand to be prepared</param>
    /// <param name="connection">A valid SqlConnection, on which to execute this command</param>
    /// <param name="transaction">A valid SqlTransaction, or 'null'</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
    /// <param name="mustCloseConnection"><c>true</c> if the connection was opened by the method, otherwose is false.</param>
    private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, out bool mustCloseConnection)
    {
        if (command == null) throw new ArgumentNullException("command");
        if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

        // If the provided connection is not open, we will open it
        if (connection.State != ConnectionState.Open)
        {
            mustCloseConnection = true;
            connection.Open();
        }
        else
        {
            mustCloseConnection = false;
        }

        // Associate the connection with the command
        command.Connection = connection;

        // Set the command text (stored procedure name or SQL statement)
        command.CommandText = commandText;

        // If we were provided a transaction, assign it
        if (transaction != null)
        {
            if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            command.Transaction = transaction;
        }

        // Set the command type
        command.CommandType = commandType;

        // Attach the command parameters if they are provided
        if (commandParameters != null)
        {
            AttachParameters(command, commandParameters);
        }
        return;
    }

    #endregion private utility methods & constructors

    #region ExecuteNonQuery

    public static int ExecuteNonQuery(CommandType commandType, string commandText)
    {
        return ExecuteNonQuery(_ConnectionString, commandType, commandText);
    }

    /// <summary>
    /// 默认为 CommandType.Text
    /// </summary>
    /// <param name="commandText"></param>
    /// <returns></returns>
    public static int ExecuteNonQuery(string commandText)
    {
        CommandType commandType = CommandType.Text;
        return ExecuteNonQuery(_ConnectionString, commandType, commandText);
    }

    public static int ExecuteNonQuery(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        return ExecuteNonQuery(_ConnectionString, commandType, commandText, commandParameters);
    }

    /// <summary>
    ///  默认为 CommandType.Text
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="commandParameters"></param>
    /// <returns></returns>
    public static int ExecuteNonQuery(string commandText, params SqlParameter[] commandParameters)
    {
        CommandType commandType = CommandType.Text;
        return ExecuteNonQuery(_ConnectionString, commandType, commandText, commandParameters);
    }
    /// <summary>
    /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the database specified in 
    /// the connection string
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>An int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteNonQuery(connectionString, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
    /// using the provided parameters
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>An int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

        // Create & open a SqlConnection, and dispose of it after we are done
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Call the overload that takes a connection in place of the connection string
            return ExecuteNonQuery(connection, commandType, commandText, commandParameters);
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in 
    /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored prcedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>An int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlConnection. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>An int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteNonQuery(connection, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns no resultset) against the specified SqlConnection 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>An int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (connection == null) throw new ArgumentNullException("connection");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

        // Finally, execute the command
        int retval = cmd.ExecuteNonQuery();

        // Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear();
        if (mustCloseConnection)
            connection.Close();
        return retval;
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlConnection 
    /// using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>An int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQuery(SqlConnection connection, string spName, params object[] parameterValues)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlTransaction. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>An int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteNonQuery(transaction, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns no resultset) against the specified SqlTransaction
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>An int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

        // Finally, execute the command
        int retval = cmd.ExecuteNonQuery();

        // Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear();
        return retval;
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified 
    /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>An int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQuery(SqlTransaction transaction, string spName, params object[] parameterValues)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
        }
    }

    #endregion ExecuteNonQuery

    #region ExecuteDataset

    public static DataSet ExecuteDataset(CommandType commandType, string commandText)
    {
        return ExecuteDataset(_ConnectionString, commandType, commandText);
    }
    public static DataSet ExecuteDataset(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        return ExecuteDataset(_ConnectionString, commandType, commandText, commandParameters);
    }

    /// <summary>
    /// 默认为CommandType.Text
    /// </summary>
    /// <param name="commandText"></param>
    /// <returns></returns>
    public static DataSet ExecuteDataset(string commandText)
    {
        CommandType commandType = CommandType.Text;
        return ExecuteDataset(_ConnectionString, commandType, commandText);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
    /// the connection string. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteDataset(connectionString, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

        // Create & open a SqlConnection, and dispose of it after we are done
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Call the overload that takes a connection in place of the connection string
            return ExecuteDataset(connection, commandType, commandText, commandParameters);
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
    /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteDataset(connection, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (connection == null) throw new ArgumentNullException("connection");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

        // Create the DataAdapter & DataSet
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataSet ds = new DataSet();

            // Fill the DataSet using default values for DataTable names, etc



            da.Fill(ds);

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();

            if (mustCloseConnection)
                connection.Close();

            // Return the dataset
            return ds;
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
    /// using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static DataSet ExecuteDataset(SqlConnection connection, string spName, params object[] parameterValues)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteDataset(transaction, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

        // Create the DataAdapter & DataSet
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataSet ds = new DataSet();

            // Fill the DataSet using default values for DataTable names, etc
            da.Fill(ds);

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();

            // Return the dataset
            return ds;
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
    /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static DataSet ExecuteDataset(SqlTransaction transaction, string spName, params object[] parameterValues)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
        }
    }

    #endregion ExecuteDataset

    #region ExecuteReader

    /// <summary>
    /// This enum is used to indicate whether the connection was provided by the caller, or created by SqlHelper, so that
    /// we can set the appropriate CommandBehavior when calling ExecuteReader()
    /// </summary>
    private enum SqlConnectionOwnership
    {
        /// <summary>Connection is owned and managed by SqlHelper</summary>
        Internal,
        /// <summary>Connection is owned and managed by the caller</summary>
        External
    }

    public static SqlDataReader ExecuteReader(CommandType commandType, string commandText)
    {
        return ExecuteReader(_ConnectionString, commandType, commandText);
    }
    public static SqlDataReader ExecuteReader(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        return ExecuteReader(_ConnectionString, commandType, commandText, commandParameters);
    }

    /// <summary>
    /// Create and prepare a SqlCommand, and call ExecuteReader with the appropriate CommandBehavior.
    /// </summary>
    /// <remarks>
    /// If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
    /// 
    /// If the caller provided the connection, we want to leave it to them to manage.
    /// </remarks>
    /// <param name="connection">A valid SqlConnection, on which to execute this command</param>
    /// <param name="transaction">A valid SqlTransaction, or 'null'</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
    /// <param name="connectionOwnership">Indicates whether the connection parameter was provided by the caller, or created by SqlHelper</param>
    /// <returns>SqlDataReader containing the results of the command</returns>
    private static SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, SqlConnectionOwnership connectionOwnership)
    {
        if (connection == null) throw new ArgumentNullException("connection");

        bool mustCloseConnection = false;
        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        try
        {
            PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create a reader
            SqlDataReader dataReader;

            // Call ExecuteReader with the appropriate CommandBehavior
            if (connectionOwnership == SqlConnectionOwnership.External)
            {
                dataReader = cmd.ExecuteReader();
            }
            else
            {
                dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }

            // Detach the SqlParameters from the command object, so they can be used again.
            // HACK: There is a problem here, the output parameter values are fletched 
            // when the reader is closed, so if the parameters are detached from the command
            // then the SqlReader can磘 set its values. 
            // When this happen, the parameters can磘 be used again in other command.
            bool canClear = true;
            foreach (SqlParameter commandParameter in cmd.Parameters)
            {
                if (commandParameter.Direction != ParameterDirection.Input)
                    canClear = false;
            }

            if (canClear)
            {
                cmd.Parameters.Clear();
            }

            return dataReader;
        }
        catch
        {
            if (mustCloseConnection)
                connection.Close();
            throw;
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
    /// the connection string. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
    public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteReader(connectionString, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
    public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        SqlConnection connection = null;
        try
        {
            connection = new SqlConnection(connectionString);
            connection.Open();

            // Call the private overload that takes an internally owned connection in place of the connection string
            return ExecuteReader(connection, null, commandType, commandText, commandParameters, SqlConnectionOwnership.Internal);
        }
        catch
        {
            // If we fail to return the SqlDatReader, we need to close the connection ourselves
            if (connection != null) connection.Close();
            throw;
        }

    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
    /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  SqlDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
    public static SqlDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

            AssignParameterValues(commandParameters, parameterValues);

            return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
    public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteReader(connection, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
    public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        // Pass through the call to the private overload using a null transaction value and an externally owned connection
        return ExecuteReader(connection, (SqlTransaction)null, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
    /// using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  SqlDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
    public static SqlDataReader ExecuteReader(SqlConnection connection, string spName, params object[] parameterValues)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

            AssignParameterValues(commandParameters, parameterValues);

            return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteReader(connection, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
    public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteReader(transaction, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///   SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
    public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

        // Pass through to private overload, indicating that the connection is owned by the caller
        return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified
    /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  SqlDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
    public static SqlDataReader ExecuteReader(SqlTransaction transaction, string spName, params object[] parameterValues)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

            AssignParameterValues(commandParameters, parameterValues);

            return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
        }
    }

    #endregion ExecuteReader

    #region ExecuteScalar
    public static object ExecuteScalar(string commandText)
    {
        return ExecuteScalar(CommandType.Text, commandText);
    }

    public static object ExecuteScalar(CommandType commandType, string commandText)
    {
        return ExecuteScalar(_ConnectionString, commandType, commandText);
    }
    public static object ExecuteScalar(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        return ExecuteScalar(_ConnectionString, commandType, commandText, commandParameters);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
    /// the connection string. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteScalar(connectionString, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a 1x1 resultset) against the database specified in the connection string 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        // Create & open a SqlConnection, and dispose of it after we are done
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Call the overload that takes a connection in place of the connection string
            return ExecuteScalar(connection, commandType, commandText, commandParameters);
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in 
    /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlConnection. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteScalar(connection, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (connection == null) throw new ArgumentNullException("connection");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();

        bool mustCloseConnection = false;
        PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

        // Execute the command & return the results
        object retval = cmd.ExecuteScalar();

        // Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear();

        if (mustCloseConnection)
            connection.Close();

        return retval;
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
    /// using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalar(SqlConnection connection, string spName, params object[] parameterValues)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlTransaction. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteScalar(transaction, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

        // Execute the command & return the results
        object retval = cmd.ExecuteScalar();

        // Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear();
        return retval;
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified
    /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalar(SqlTransaction transaction, string spName, params object[] parameterValues)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // PPull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
        }
    }

    #endregion ExecuteScalar

    #region ExecuteXmlReader
    /// <summary>
    /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders");
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
    /// <returns>An XmlReader containing the resultset generated by the command</returns>
    public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteXmlReader(connection, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>An XmlReader containing the resultset generated by the command</returns>
    public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (connection == null) throw new ArgumentNullException("connection");

        bool mustCloseConnection = false;
        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        try
        {
            PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            XmlReader retval = cmd.ExecuteXmlReader();

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();

            return retval;
        }
        catch
        {
            if (mustCloseConnection)
                connection.Close();
            throw;
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
    /// using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  XmlReader r = ExecuteXmlReader(conn, "GetOrders", 24, 36);
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="spName">The name of the stored procedure using "FOR XML AUTO"</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>An XmlReader containing the resultset generated by the command</returns>
    public static XmlReader ExecuteXmlReader(SqlConnection connection, string spName, params object[] parameterValues)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
    /// <returns>An XmlReader containing the resultset generated by the command</returns>
    public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText)
    {
        // Pass through the call providing null for the set of SqlParameters
        return ExecuteXmlReader(transaction, commandType, commandText, (SqlParameter[])null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <returns>An XmlReader containing the resultset generated by the command</returns>
    public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

        // Create a command and prepare it for execution
        SqlCommand cmd = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

        // Create the DataAdapter & DataSet
        XmlReader retval = cmd.ExecuteXmlReader();

        // Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear();
        return retval;
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
    /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  XmlReader r = ExecuteXmlReader(trans, "GetOrders", 24, 36);
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static XmlReader ExecuteXmlReader(SqlTransaction transaction, string spName, params object[] parameterValues)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
        }
    }

    #endregion ExecuteXmlReader

    #region FillDataset
    /// <summary>
    /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
    /// the connection string. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    /// by a user defined name (probably the actual table name)</param>
    public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (dataSet == null) throw new ArgumentNullException("dataSet");

        // Create & open a SqlConnection, and dispose of it after we are done
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Call the overload that takes a connection in place of the connection string
            FillDataset(connection, commandType, commandText, dataSet, tableNames);
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    /// by a user defined name (probably the actual table name)
    /// </param>
    public static void FillDataset(string connectionString, CommandType commandType,
        string commandText, DataSet dataSet, string[] tableNames,
        params SqlParameter[] commandParameters)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (dataSet == null) throw new ArgumentNullException("dataSet");
        // Create & open a SqlConnection, and dispose of it after we are done
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Call the overload that takes a connection in place of the connection string
            FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters);
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
    /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    /// by a user defined name (probably the actual table name)
    /// </param>    
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    public static void FillDataset(string connectionString, string spName,
        DataSet dataSet, string[] tableNames,
        params object[] parameterValues)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (dataSet == null) throw new ArgumentNullException("dataSet");
        // Create & open a SqlConnection, and dispose of it after we are done
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Call the overload that takes a connection in place of the connection string
            FillDataset(connection, spName, dataSet, tableNames, parameterValues);
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    /// by a user defined name (probably the actual table name)
    /// </param>    
    public static void FillDataset(SqlConnection connection, CommandType commandType,
        string commandText, DataSet dataSet, string[] tableNames)
    {
        FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    /// by a user defined name (probably the actual table name)
    /// </param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    public static void FillDataset(SqlConnection connection, CommandType commandType,
        string commandText, DataSet dataSet, string[] tableNames,
        params SqlParameter[] commandParameters)
    {
        FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
    /// using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    /// by a user defined name (probably the actual table name)
    /// </param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    public static void FillDataset(SqlConnection connection, string spName,
        DataSet dataSet, string[] tableNames,
        params object[] parameterValues)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (dataSet == null) throw new ArgumentNullException("dataSet");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
        }
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    /// by a user defined name (probably the actual table name)
    /// </param>
    public static void FillDataset(SqlTransaction transaction, CommandType commandType,
        string commandText,
        DataSet dataSet, string[] tableNames)
    {
        FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
    }

    /// <summary>
    /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    /// by a user defined name (probably the actual table name)
    /// </param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    public static void FillDataset(SqlTransaction transaction, CommandType commandType,
        string commandText, DataSet dataSet, string[] tableNames,
        params SqlParameter[] commandParameters)
    {
        FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
    /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <remarks>
    /// This method provides no access to output parameters or the stored procedure's return value parameter.
    /// 
    /// e.g.:  
    ///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
    /// </remarks>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    /// by a user defined name (probably the actual table name)
    /// </param>
    /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
    public static void FillDataset(SqlTransaction transaction, string spName,
        DataSet dataSet, string[] tableNames,
        params object[] parameterValues)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        if (dataSet == null) throw new ArgumentNullException("dataSet");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If we receive parameter values, we need to figure out where they go
        if ((parameterValues != null) && (parameterValues.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

            // Assign the provided values to these parameters based on parameter order
            AssignParameterValues(commandParameters, parameterValues);

            // Call the overload that takes an array of SqlParameters
            FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
        }
        else
        {
            // Otherwise we can just call the SP without params
            FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
        }
    }

    /// <summary>
    /// Private helper method that execute a SqlCommand (that returns a resultset) against the specified SqlTransaction and SqlConnection
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  FillDataset(conn, trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="connection">A valid SqlConnection</param>
    /// <param name="transaction">A valid SqlTransaction</param>
    /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    /// by a user defined name (probably the actual table name)
    /// </param>
    /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    private static void FillDataset(SqlConnection connection, SqlTransaction transaction, CommandType commandType,
        string commandText, DataSet dataSet, string[] tableNames,
        params SqlParameter[] commandParameters)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (dataSet == null) throw new ArgumentNullException("dataSet");

        // Create a command and prepare it for execution
        SqlCommand command = new SqlCommand();
        bool mustCloseConnection = false;
        PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

        // Create the DataAdapter & DataSet
        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
        {

            // Add the table mappings specified by the user
            if (tableNames != null && tableNames.Length > 0)
            {
                string tableName = "Table";
                for (int index = 0; index < tableNames.Length; index++)
                {
                    if (tableNames[index] == null || tableNames[index].Length == 0) throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                    dataAdapter.TableMappings.Add(tableName, tableNames[index]);
                    tableName += (index + 1).ToString();
                }
            }

            // Fill the DataSet using default values for DataTable names, etc
            dataAdapter.Fill(dataSet);

            // Detach the SqlParameters from the command object, so they can be used again
            command.Parameters.Clear();
        }

        if (mustCloseConnection)
            connection.Close();
    }
    #endregion

    #region UpdateDataset
    /// <summary>
    /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
    /// </remarks>
    /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source</param>
    /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source</param>
    /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source</param>
    /// <param name="dataSet">The DataSet used to update the data source</param>
    /// <param name="tableName">The DataTable used to update the data source.</param>
    public static void UpdateDataset(SqlCommand insertCommand, SqlCommand deleteCommand, SqlCommand updateCommand, DataSet dataSet, string tableName)
    {
        if (insertCommand == null) throw new ArgumentNullException("insertCommand");
        if (deleteCommand == null) throw new ArgumentNullException("deleteCommand");
        if (updateCommand == null) throw new ArgumentNullException("updateCommand");
        if (tableName == null || tableName.Length == 0) throw new ArgumentNullException("tableName");

        // Create a SqlDataAdapter, and dispose of it after we are done
        using (SqlDataAdapter dataAdapter = new SqlDataAdapter())
        {
            // Set the data adapter commands
            dataAdapter.UpdateCommand = updateCommand;
            dataAdapter.InsertCommand = insertCommand;
            dataAdapter.DeleteCommand = deleteCommand;

            // Update the dataset changes in the data source
            dataAdapter.Update(dataSet, tableName);

            // Commit all the changes made to the DataSet
            dataSet.AcceptChanges();
        }
    }
    #endregion

    #region CreateCommand
    /// <summary>
    /// Simplify the creation of a Sql command object by allowing
    /// a stored procedure and optional parameters to be provided
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  SqlCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
    /// </remarks>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters</param>
    /// <returns>A valid SqlCommand object</returns>
    public static SqlCommand CreateCommand(SqlConnection connection, string spName, params string[] sourceColumns)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // Create a SqlCommand
        SqlCommand cmd = new SqlCommand(spName, connection);
        cmd.CommandType = CommandType.StoredProcedure;

        // If we receive parameter values, we need to figure out where they go
        if ((sourceColumns != null) && (sourceColumns.Length > 0))
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

            // Assign the provided source columns to these parameters based on parameter order
            for (int index = 0; index < sourceColumns.Length; index++)
                commandParameters[index].SourceColumn = sourceColumns[index];

            // Attach the discovered parameters to the SqlCommand object
            AttachParameters(cmd, commandParameters);
        }

        return cmd;
    }
    #endregion

    #region ExecuteNonQueryTypedParams
    public static int ExecuteNonQueryTypedParams(String spName, DataRow dataRow)
    {
        return ExecuteNonQueryTypedParams(_ConnectionString, spName, dataRow);
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in 
    /// the connection string using the dataRow column values as the stored procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
    /// </summary>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>An int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQueryTypedParams(String connectionString, String spName, DataRow dataRow)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlConnection 
    /// using the dataRow column values as the stored procedure's parameters values.  
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
    /// </summary>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>An int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQueryTypedParams(SqlConnection connection, String spName, DataRow dataRow)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified
    /// SqlTransaction using the dataRow column values as the stored procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
    /// </summary>
    /// <param name="transaction">A valid SqlTransaction object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>An int representing the number of rows affected by the command</returns>
    public static int ExecuteNonQueryTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // Sf the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
        }
    }
    #endregion

    #region ExecuteDatasetTypedParams

    public static DataSet ExecuteDatasetTypedParams(String spName, DataRow dataRow)
    {
        return ExecuteDatasetTypedParams(_ConnectionString, spName, dataRow);
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
    /// the connection string using the dataRow column values as the stored procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
    /// </summary>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static DataSet ExecuteDatasetTypedParams(string connectionString, String spName, DataRow dataRow)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        //If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
    /// using the dataRow column values as the store procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
    /// </summary>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static DataSet ExecuteDatasetTypedParams(SqlConnection connection, String spName, DataRow dataRow)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction 
    /// using the dataRow column values as the stored procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
    /// </summary>
    /// <param name="transaction">A valid SqlTransaction object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>A dataset containing the resultset generated by the command</returns>
    public static DataSet ExecuteDatasetTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
        }
    }

    #endregion

    #region ExecuteReaderTypedParams

    public static SqlDataReader ExecuteReaderTypedParams(String spName, DataRow dataRow)
    {
        return ExecuteReaderTypedParams(_ConnectionString, spName, dataRow);
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
    /// the connection string using the dataRow column values as the stored procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
    public static SqlDataReader ExecuteReaderTypedParams(String connectionString, String spName, DataRow dataRow)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
        }
    }


    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
    /// using the dataRow column values as the stored procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
    public static SqlDataReader ExecuteReaderTypedParams(SqlConnection connection, String spName, DataRow dataRow)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction 
    /// using the dataRow column values as the stored procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <param name="transaction">A valid SqlTransaction object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
    public static SqlDataReader ExecuteReaderTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName);
        }
    }
    #endregion

    #region ExecuteScalarTypedParams

    public static object ExecuteScalarTypedParams(String spName, DataRow dataRow)
    {
        return ExecuteScalarTypedParams(_ConnectionString, spName, dataRow);
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in 
    /// the connection string using the dataRow column values as the stored procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalarTypedParams(String connectionString, String spName, DataRow dataRow)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
    /// using the dataRow column values as the stored procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalarTypedParams(SqlConnection connection, String spName, DataRow dataRow)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction
    /// using the dataRow column values as the stored procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <param name="transaction">A valid SqlTransaction object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    public static object ExecuteScalarTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
        }
    }
    #endregion

    #region ExecuteXmlReaderTypedParams
    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
    /// using the dataRow column values as the stored procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>An XmlReader containing the resultset generated by the command</returns>
    public static XmlReader ExecuteXmlReaderTypedParams(SqlConnection connection, String spName, DataRow dataRow)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
        }
    }

    /// <summary>
    /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction 
    /// using the dataRow column values as the stored procedure's parameters values.
    /// This method will query the database to discover the parameters for the 
    /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
    /// </summary>
    /// <param name="transaction">A valid SqlTransaction object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
    /// <returns>An XmlReader containing the resultset generated by the command</returns>
    public static XmlReader ExecuteXmlReaderTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
    {
        if (transaction == null) throw new ArgumentNullException("transaction");
        if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        // If the row has values, the store procedure parameters must be initialized
        if (dataRow != null && dataRow.ItemArray.Length > 0)
        {
            // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
            SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

            // Set the parameters values
            AssignParameterValues(commandParameters, dataRow);

            return SqlHelper.ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
        }
        else
        {
            return SqlHelper.ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
        }
    }
    #endregion

    public static SqlParameter MakeInParameter(string parameterName, SqlDbType dbType, int size, object parameterValue)
    {
        SqlParameter parameter = new SqlParameter();
        parameter.ParameterName = parameterName;
        parameter.SqlDbType = dbType;
        parameter.Size = size;
        parameter.Direction = ParameterDirection.Input;
        parameter.Value = parameterValue;

        return parameter;
    }

    public static SqlParameter MakeInParameter(string parameterName, SqlDbType dbType, object parameterValue)
    {
        SqlParameter parameter = new SqlParameter();
        parameter.ParameterName = parameterName;
        parameter.SqlDbType = dbType;
        parameter.Direction = ParameterDirection.Input;
        parameter.Value = parameterValue;

        return parameter;
    }

    public static SqlParameter MakeOutParameter(string parameterName, SqlDbType dbType, int length)
    {
        SqlParameter parameter = new SqlParameter();
        parameter.ParameterName = parameterName;
        parameter.SqlDbType = dbType;
        parameter.Direction = ParameterDirection.Output;
        if (dbType != SqlDbType.DateTime)
            parameter.Size = length;

        return parameter;
    }
    #region Zhouxu_2016/9/12 汉字转拼音 //////////////////////////////////////////////////////////
    private static int[] pyValue = new int[]
    {
    -20319,-20317,-20304,-20295,-20292,-20283,-20265,-20257,-20242,-20230,-20051,-20036,
    -20032,-20026,-20002,-19990,-19986,-19982,-19976,-19805,-19784,-19775,-19774,-19763,
    -19756,-19751,-19746,-19741,-19739,-19728,-19725,-19715,-19540,-19531,-19525,-19515,
    -19500,-19484,-19479,-19467,-19289,-19288,-19281,-19275,-19270,-19263,-19261,-19249,
    -19243,-19242,-19238,-19235,-19227,-19224,-19218,-19212,-19038,-19023,-19018,-19006,
    -19003,-18996,-18977,-18961,-18952,-18783,-18774,-18773,-18763,-18756,-18741,-18735,
    -18731,-18722,-18710,-18697,-18696,-18526,-18518,-18501,-18490,-18478,-18463,-18448,
    -18447,-18446,-18239,-18237,-18231,-18220,-18211,-18201,-18184,-18183, -18181,-18012,
    -17997,-17988,-17970,-17964,-17961,-17950,-17947,-17931,-17928,-17922,-17759,-17752,
    -17733,-17730,-17721,-17703,-17701,-17697,-17692,-17683,-17676,-17496,-17487,-17482,
    -17468,-17454,-17433,-17427,-17417,-17202,-17185,-16983,-16970,-16942,-16915,-16733,
    -16708,-16706,-16689,-16664,-16657,-16647,-16474,-16470,-16465,-16459,-16452,-16448,
    -16433,-16429,-16427,-16423,-16419,-16412,-16407,-16403,-16401,-16393,-16220,-16216,
    -16212,-16205,-16202,-16187,-16180,-16171,-16169,-16158,-16155,-15959,-15958,-15944,
    -15933,-15920,-15915,-15903,-15889,-15878,-15707,-15701,-15681,-15667,-15661,-15659,
    -15652,-15640,-15631,-15625,-15454,-15448,-15436,-15435,-15419,-15416,-15408,-15394,
    -15385,-15377,-15375,-15369,-15363,-15362,-15183,-15180,-15165,-15158,-15153,-15150,
    -15149,-15144,-15143,-15141,-15140,-15139,-15128,-15121,-15119,-15117,-15110,-15109,
    -14941,-14937,-14933,-14930,-14929,-14928,-14926,-14922,-14921,-14914,-14908,-14902,
    -14894,-14889,-14882,-14873,-14871,-14857,-14678,-14674,-14670,-14668,-14663,-14654,
    -14645,-14630,-14594,-14429,-14407,-14399,-14384,-14379,-14368,-14355,-14353,-14345,
    -14170,-14159,-14151,-14149,-14145,-14140,-14137,-14135,-14125,-14123,-14122,-14112,
    -14109,-14099,-14097,-14094,-14092,-14090,-14087,-14083,-13917,-13914,-13910,-13907,
    -13906,-13905,-13896,-13894,-13878,-13870,-13859,-13847,-13831,-13658,-13611,-13601,
    -13406,-13404,-13400,-13398,-13395,-13391,-13387,-13383,-13367,-13359,-13356,-13343,
    -13340,-13329,-13326,-13318,-13147,-13138,-13120,-13107,-13096,-13095,-13091,-13076,
    -13068,-13063,-13060,-12888,-12875,-12871,-12860,-12858,-12852,-12849,-12838,-12831,
    -12829,-12812,-12802,-12607,-12597,-12594,-12585,-12556,-12359,-12346,-12320,-12300,
    -12120,-12099,-12089,-12074,-12067,-12058,-12039,-11867,-11861,-11847,-11831,-11798,
    -11781,-11604,-11589,-11536,-11358,-11340,-11339,-11324,-11303,-11097,-11077,-11067,
    -11055,-11052,-11045,-11041,-11038,-11024,-11020,-11019,-11018,-11014,-10838,-10832,
    -10815,-10800,-10790,-10780,-10764,-10587,-10544,-10533,-10519,-10331,-10329,-10328,
    -10322,-10315,-10309,-10307,-10296,-10281,-10274,-10270,-10262,-10260,-10256,-10254
    };
    private static string[] pyName = new string[]
{
    "A","Ai","An","Ang","Ao","Ba","Bai","Ban","Bang","Bao","Bei","Ben",
    "Beng","Bi","Bian","Biao","Bie","Bin","Bing","Bo","Bu","Ba","Cai","Can",
    "Cang","Cao","Ce","Ceng","Cha","Chai","Chan","Chang","Chao","Che","Chen","Cheng",
    "Chi","Chong","Chou","Chu","Chuai","Chuan","Chuang","Chui","Chun","Chuo","Ci","Cong",
    "Cou","Cu","Cuan","Cui","Cun","Cuo","Da","Dai","Dan","Dang","Dao","De",
    "Deng","Di","Dian","Diao","Die","Ding","Diu","Dong","Dou","Du","Duan","Dui",
    "Dun","Duo","E","En","Er","Fa","Fan","Fang","Fei","Fen","Feng","Fo",
    "Fou","Fu","Ga","Gai","Gan","Gang","Gao","Ge","Gei","Gen","Geng","Gong",
    "Gou","Gu","Gua","Guai","Guan","Guang","Gui","Gun","Guo","Ha","Hai","Han",
    "Hang","Hao","He","Hei","Hen","Heng","Hong","Hou","Hu","Hua","Huai","Huan",
    "Huang","Hui","Hun","Huo","Ji","Jia","Jian","Jiang","Jiao","Jie","Jin","Jing",
    "Jiong","Jiu","Ju","Juan","Jue","Jun","Ka","Kai","Kan","Kang","Kao","Ke",
    "Ken","Keng","Kong","Kou","Ku","Kua","Kuai","Kuan","Kuang","Kui","Kun","Kuo",
    "La","Lai","Lan","Lang","Lao","Le","Lei","Leng","Li","Lia","Lian","Liang",
    "Liao","Lie","Lin","Ling","Liu","Long","Lou","Lu","Lv","Luan","Lue","Lun",
    "Luo","Ma","Mai","Man","Mang","Mao","Me","Mei","Men","Meng","Mi","Mian",
    "Miao","Mie","Min","Ming","Miu","Mo","Mou","Mu","Na","Nai","Nan","Nang",
    "Nao","Ne","Nei","Nen","Neng","Ni","Nian","Niang","Niao","Nie","Nin","Ning",
    "Niu","Nong","Nu","Nv","Nuan","Nue","Nuo","O","Ou","Pa","Pai","Pan",
    "Pang","Pao","Pei","Pen","Peng","Pi","Pian","Piao","Pie","Pin","Ping","Po",
    "Pu","Qi","Qia","Qian","Qiang","Qiao","Qie","Qin","Qing","Qiong","Qiu","Qu",
    "Quan","Que","Qun","Ran","Rang","Rao","Re","Ren","Reng","Ri","Rong","Rou",
    "Ru","Ruan","Rui","Run","Ruo","Sa","Sai","San","Sang","Sao","Se","Sen",
    "Seng","Sha","Shai","Shan","Shang","Shao","She","Shen","Sheng","Shi","Shou","Shu",
    "Shua","Shuai","Shuan","Shuang","Shui","Shun","Shuo","Si","Song","Sou","Su","Suan",
    "Sui","Sun","Suo","Ta","Tai","Tan","Tang","Tao","Te","Teng","Ti","Tian",
    "Tiao","Tie","Ting","Tong","Tou","Tu","Tuan","Tui","Tun","Tuo","Wa","Wai",
    "Wan","Wang","Wei","Wen","Weng","Wo","Wu","Xi","Xia","Xian","Xiang","Xiao",
    "Xie","Xin","Xing","Xiong","Xiu","Xu","Xuan","Xue","Xun","Ya","Yan","Yang",
    "Yao","Ye","Yi","Yin","Ying","Yo","Yong","You","Yu","Yuan","Yue","Yun",
    "Za", "Zai","Zan","Zang","Zao","Ze","Zei","Zen","Zeng","Zha","Zhai","Zhan",
    "Zhang","Zhao","Zhe","Zhen","Zheng","Zhi","Zhong","Zhou","Zhu","Zhua","Zhuai","Zhuan",
    "Zhuang","Zhui","Zhun","Zhuo","Zi","Zong","Zou","Zu","Zuan","Zui","Zun","Zuo"
};
    /// <summary>  
    /// 把汉字转换成拼音(全拼) Zhouxu_2016/9/12   
    /// </summary>  
    /// <param name="transName">汉字字符串</param>  
    /// <returns>转换后的拼音(全拼)字符串</returns>  
    public static string GetAllPYLetters(string transName)
    {
        // 匹配中文字符  
        Regex regex = new Regex(@"[\u4e00-\u9fbb]");
        byte[] array = new byte[2];
        string pyString = "";
        int chrAsc = 0;
        int i1 = 0;
        int i2 = 0;
        char[] noWChar = transName.ToCharArray();
        for (int j = 0; j < noWChar.Length; j++)
        {
            // 中文字符  
            if (regex.IsMatch(noWChar[j].ToString()))
            {
                array = System.Text.Encoding.Default.GetBytes(noWChar[j].ToString());
                i1 = (short)(array[0]);
                i2 = (short)(array[1]);
                chrAsc = i1 * 256 + i2 - 65536;
                if (chrAsc > 0 && chrAsc < 160)
                {
                    pyString += noWChar[j];
                }
                else
                {
                    // 修正部分文字  
                    if (chrAsc == -9254)  // 修正“圳”字  
                        pyString += "Zhen";
                    else
                    {
                        for (int i = (pyValue.Length - 1); i >= 0; i--)
                        {
                            if (pyValue[i] <= chrAsc)
                            {
                                pyString += pyName[i];
                                break;
                            }
                        }
                    }
                }
            }
            // 非中文字符  
            else
            {
                pyString += noWChar[j].ToString();
            }
        }
        return pyString;
    }
    /// <summary>  
    /// 从字符串中获取汉字拼音首字母，不是汉字则原样输出  Zhouxu_2016/9/12 
    /// </summary>  
    /// <param name="transName"></param>  
    /// <returns></returns>  
    public static string GetFirstPYLetter(string transName)
    {
        string ls_second_eng = "XQLWCJWGNSPGCGNESYPBTYYZDXYKYGTDJNNJQMBSGZSCYJSYYQPGKBZGYCYWJKGKLJSWKPJQHYTWDDZLSGMRYPYWWCCKZNKYDGTTNGJEYKKZYTCJNMCYLQLYPYQFQRPZSLWBTGKJFYXJWZLTBNCXJJJJZXDTTSQZYCDXXHGCKBPHFFSSWYBGMXLPBYLLLHLXSPZMYJHSOJNGHDZQYKLGJHSGQZHXQGKEZZWYSCSCJXYEYXADZPMDSSMZJZQJYZCDJZWQJBDZBXGZNZCPWHKXHQKMWFBPBYDTJZZKQHYLYGXFPTYJYYZPSZLFCHMQSHGMXXSXJJSDCSBBQBEFSJYHWWGZKPYLQBGLDLCCTNMAYDDKSSNGYCSGXLYZAYBNPTSDKDYLHGYMYLCXPYCJNDQJWXQXFYYFJLEJBZRXCCQWQQSBNKYMGPLBMJRQCFLNYMYQMSQTRBCJTHZTQFRXQ" +
      "HXMJJCJLXQGJMSHZKBSWYEMYLTXFSYDSGLYCJQXSJNQBSCTYHBFTDCYZDJWYGHQFRXWCKQKXEBPTLPXJZSRMEBWHJLBJSLYYSMDXLCLQKXLHXJRZJMFQHXHWYWSBHTRXXGLHQHFNMNYKLDYXZPWLGGTMTCFPAJJZYLJTYANJGBJPLQGDZYQYAXBKYSECJSZNSLYZHZXLZCGHPXZHZNYTDSBCJKDLZAYFMYDLEBBGQYZKXGLDNDNYSKJSHDLYXBCGHXYPKDJMMZNGMMCLGWZSZXZJFZNMLZZTHCSYDBDLLSCDDNLKJYKJSYCJLKOHQASDKNHCSGANHDAASHTCPLCPQYBSDMPJLPCJOQLCDHJJYSPRCHNWJNLHLYYQYYWZPTCZGWWMZFFJQQQQYXACLBHKDJXDGMMYDJXZLLSYGXGKJRYWZWYCLZMSSJZLDBYDCFCXYHLXCHYZJQSFQAGMNYXPFRKSSB" +
      "JLYXYSYGLNSCMHCWWMNZJJLXXHCHSYDSTTXRYCYXBYHCSMXJSZNPWGPXXTAYBGAJCXLYSDCCWZOCWKCCSBNHCPDYZNFCYYTYCKXKYBSQKKYTQQXFCWCHCYKELZQBSQYJQCCLMTHSYWHMKTLKJLYCXWHEQQHTQHZPQSQSCFYMMDMGBWHWLGSSLYSDLMLXPTHMJHWLJZYHZJXHTXJLHXRSWLWZJCBXMHZQXSDZPMGFCSGLSXYMJSHXPJXWMYQKSMYPLRTHBXFTPMHYXLCHLHLZYLXGSSSSTCLSLDCLRPBHZHXYYFHBBGDMYCNQQWLQHJJZYWJZYEJJDHPBLQXTQKWHLCHQXAGTLXLJXMSLXHTZKZJECXJCJNMFBYCSFYWYBJZGNYSDZSQYRSLJPCLPWXSDWEJBJCBCNAYTWGMPAPCLYQPCLZXSBNMSGGFNZJJBZSFZYNDXHPLQKZCZWALSBCCJXJYZGWKYP" +
      "SGXFZFCDKHJGXDLQFSGDSLQWZKXTMHSBGZMJZRGLYJBPMLMSXLZJQQHZYJCZYDJWBMJKLDDPMJEGXYHYLXHLQYQHKYCWCJMYYXNATJHYCCXZPCQLBZWWYTWBQCMLPMYRJCCCXFPZNZZLJPLXXYZTZLGDLDCKLYRZZGQTGJHHHJLJAXFGFJZSLCFDQZLCLGJDJCSNCLLJPJQDCCLCJXMYZFTSXGCGSBRZXJQQCTZHGYQTJQQLZXJYLYLBCYAMCSTYLPDJBYREGKLZYZHLYSZQLZNWCZCLLWJQJJJKDGJZOLBBZPPGLGHTGZXYGHZMYCNQSYCYHBHGXKAMTXYXNBSKYZZGJZLQJDFCJXDYGJQJJPMGWGJJJPKQSBGBMMCJSSCLPQPDXCDYYKYFCJDDYYGYWRHJRTGZNYQLDKLJSZZGZQZJGDYKSHPZMTLCPWNJAFYZDJCNMWESCYGLBTZCGMSSLLYXQSXSBSJS" +
      "BBSGGHFJLWPMZJNLYYWDQSHZXTYYWHMCYHYWDBXBTLMSYYYFSXJCSDXXLHJHFSSXZQHFZMZCZTQCXZXRTTDJHNNYZQQMNQDMMGYYDXMJGDHCDYZBFFALLZTDLTFXMXQZDNGWQDBDCZJDXBZGSQQDDJCMBKZFFXMKDMDSYYSZCMLJDSYNSPRSKMKMPCKLGDBQTFZSWTFGGLYPLLJZHGJJGYPZLTCSMCNBTJBQFKTHBYZGKPBBYMTTSSXTBNPDKLEYCJNYCDYKZDDHQHSDZSCTARLLTKZLGECLLKJLQJAQNBDKKGHPJTZQKSECSHALQFMMGJNLYJBBTMLYZXDCJPLDLPCQDHZYCBZSCZBZMSLJFLKRZJSNFRGJHXPDHYJYBZGDLQCSEZGXLBLGYXTWMABCHECMWYJYZLLJJYHLGBDJLSLYGKDZPZXJYYZLWCXSZFGWYYDLYHCLJSCMBJHBLYZLYCBLYDPDQYSXQZB" +
      "YTDKYXJYYCNRJMDJGKLCLJBCTBJDDBBLBLCZQRPXJCGLZCSHLTOLJNMDDDLNGKAQHQHJGYKHEZNMSHRPHQQJCHGMFPRXHJGDYCHGHLYRZQLCYQJNZSQTKQJYMSZSWLCFQQQXYFGGYPTQWLMCRNFKKFSYYLQBMQAMMMYXCTPSHCPTXXZZSM" + "ALBXYFBPNLSFHTGJWEJJXXGLLJSTGSHJQLZFKCGNNDSZFDEQFHBSAQTGLLBXMMYGSZLDYDQMJJRGBJTKGDHGKBLQKBDMBYLXWCXYTTYBKMRTJZXQJBHLMHMJJZMQASLDCYXYQDLQCAFYWYXQHZ";
        string ls_second_ch = "鑫瞿麟雯亍丌兀丐廿卅丕亘丞鬲孬噩丨禺丿匕乇夭爻卮氐囟胤馗毓睾鼗丶亟" +
      "鼐乜乩亓芈孛啬嘏仄厍厝厣厥厮靥赝匚叵匦匮匾赜卦卣刂刈刎刭刳刿剀剌剞剡剜蒯剽劂劁劐劓冂罔亻仃仉仂仨仡仫仞伛仳伢佤仵伥伧伉伫佞佧攸佚佝佟佗伲伽佶佴侑侉侃侏佾佻侪佼侬侔俦俨俪俅俚俣俜俑俟俸倩偌俳倬倏倮倭俾倜倌倥倨偾偃偕偈偎偬偻傥傧傩傺僖儆僭僬僦僮儇儋仝氽佘佥俎龠氽籴兮巽黉馘冁夔勹匍訇匐凫夙兕亠兖亳衮袤亵脔裒禀嬴蠃羸冫冱冽冼凇冖冢冥讠讦讧讪讴讵讷诂诃诋诏诎诒诓诔诖诘诙诜诟诠诤诨诩诮诰诳诶诹诼诿谀谂谄谇谌谏谑谒谔谕谖谙谛谘谝谟谠谡谥谧谪谫谮谯谲谳谵谶卩卺阝阢阡阱阪阽阼" +
      "陂陉陔陟陧陬陲陴隈隍隗隰邗邛邝邙邬邡邴邳邶邺邸邰郏郅邾郐郄郇郓郦郢郜郗郛郫郯郾鄄鄢鄞鄣鄱鄯鄹酃酆刍奂劢劬劭劾哿勐勖勰叟燮矍廴凵凼鬯厶弁畚巯坌垩垡塾墼壅壑圩圬圪圳圹圮圯坜圻坂坩垅坫垆坼坻坨坭坶坳垭垤垌垲埏垧垴垓垠埕埘埚埙埒垸埴埯埸埤埝堋堍埽埭堀堞堙塄堠塥塬墁墉墚墀馨鼙懿艹艽艿芏芊芨芄芎芑芗芙芫芸芾芰苈苊苣芘芷芮苋苌苁芩芴芡芪芟苄苎芤苡茉苷苤茏茇苜苴苒苘茌苻苓茑茚茆茔茕苠苕茜荑荛荜茈莒茼茴茱莛荞茯荏荇荃荟荀茗荠茭茺茳荦荥荨茛荩荬荪荭荮莰荸莳莴莠莪莓莜莅荼莶莩荽莸荻" +
      "莘莞莨莺莼菁萁菥菘堇萘萋菝菽菖萜萸萑萆菔菟萏萃菸菹菪菅菀萦菰菡葜葑葚葙葳蒇蒈葺蒉葸萼葆葩葶蒌蒎萱葭蓁蓍蓐蓦蒽蓓蓊蒿蒺蓠蒡蒹蒴蒗蓥蓣蔌甍蔸蓰蔹蔟蔺蕖蔻蓿蓼蕙蕈蕨蕤蕞蕺瞢蕃蕲蕻薤薨薇薏蕹薮薜薅薹薷薰藓藁藜藿蘧蘅蘩蘖蘼廾弈夼奁耷奕奚奘匏尢尥尬尴扌扪抟抻拊拚拗拮挢拶挹捋捃掭揶捱捺掎掴捭掬掊捩掮掼揲揸揠揿揄揞揎摒揆掾摅摁搋搛搠搌搦搡摞撄摭撖摺撷撸撙撺擀擐擗擤擢攉攥攮弋忒甙弑卟叱叽叩叨叻吒吖吆呋呒呓呔呖呃吡呗呙吣吲咂咔呷呱呤咚咛咄呶呦咝哐咭哂咴哒咧咦哓哔呲咣哕咻咿哌哙哚哜咩" +
      "咪咤哝哏哞唛哧唠哽唔哳唢唣唏唑唧唪啧喏喵啉啭啁啕唿啐唼唷啖啵啶啷唳唰啜喋嗒喃喱喹喈喁喟啾嗖喑啻嗟喽喾喔喙嗪嗷嗉嘟嗑嗫嗬嗔嗦嗝嗄嗯嗥嗲嗳嗌嗍嗨嗵嗤辔嘞嘈嘌嘁嘤嘣嗾嘀嘧嘭噘嘹噗嘬噍噢噙噜噌噔嚆噤噱噫噻噼嚅嚓嚯囔囗囝囡囵囫囹囿圄圊圉圜帏帙帔帑帱帻帼帷幄幔幛幞幡岌屺岍岐岖岈岘岙岑岚岜岵岢岽岬岫岱岣峁岷峄峒峤峋峥崂崃崧崦崮崤崞崆崛嵘崾崴崽嵬嵛嵯嵝嵫嵋嵊嵩嵴嶂嶙嶝豳嶷巅彳彷徂徇徉後徕徙徜徨徭徵徼衢彡犭犰犴犷犸狃狁狎狍狒狨狯狩狲狴狷猁狳猃狺狻猗猓猡猊猞猝猕猢猹猥猬猸猱獐獍獗獠獬獯獾" +
      "舛夥飧夤夂饣饧饨饩饪饫饬饴饷饽馀馄馇馊馍馐馑馓馔馕庀庑庋庖庥庠庹庵庾庳赓廒廑廛廨廪膺忄忉忖忏怃忮怄忡忤忾怅怆忪忭忸怙怵怦怛怏怍怩怫怊怿怡恸恹恻恺恂恪恽悖悚悭悝悃悒悌悛惬悻悱惝惘惆惚悴愠愦愕愣惴愀愎愫慊慵憬憔憧憷懔懵忝隳闩闫闱闳闵闶闼闾阃阄阆阈阊阋阌阍阏阒阕阖阗阙阚丬爿戕氵汔汜汊沣沅沐沔沌汨汩汴汶沆沩泐泔沭泷泸泱泗沲泠泖泺泫泮沱泓泯泾洹洧洌浃浈洇洄洙洎洫浍洮洵洚浏浒浔洳涑浯涞涠浞涓涔浜浠浼浣渚淇淅淞渎涿淠渑淦淝淙渖涫渌涮渫湮湎湫溲湟溆湓湔渲渥湄滟溱溘滠漭滢溥溧溽溻溷滗溴滏溏滂" +
      "溟潢潆潇漤漕滹漯漶潋潴漪漉漩澉澍澌潸潲潼潺濑濉澧澹澶濂濡濮濞濠濯瀚瀣瀛瀹瀵灏灞宀宄宕宓宥宸甯骞搴寤寮褰寰蹇謇辶迓迕迥迮迤迩迦迳迨逅逄逋逦逑逍逖逡逵逶逭逯遄遑遒遐遨遘遢遛暹遴遽邂邈邃邋彐彗彖彘尻咫屐屙孱屣屦羼弪弩弭艴弼鬻屮妁妃妍妩妪妣妗姊妫妞妤姒妲妯姗妾娅娆姝娈姣姘姹娌娉娲娴娑娣娓婀婧婊婕娼婢婵胬媪媛婷婺媾嫫媲嫒嫔媸嫠嫣嫱嫖嫦嫘嫜嬉嬗嬖嬲嬷孀尕尜孚孥孳孑孓孢驵驷驸驺驿驽骀骁骅骈骊骐骒骓骖骘骛骜骝骟骠骢骣骥骧纟纡纣纥纨纩纭纰纾绀绁绂绉绋绌绐绔绗绛绠绡绨绫绮绯绱绲缍绶绺绻绾缁缂缃" + "缇缈缋缌缏缑缒缗缙缜缛缟缡缢缣缤缥缦缧缪缫缬缭缯缰缱缲缳缵幺畿巛甾邕玎玑玮玢玟珏珂珑玷玳珀珉珈珥珙顼琊珩珧珞玺珲琏琪瑛琦琥琨琰琮琬琛琚瑁瑜瑗瑕瑙瑷瑭瑾璜璎璀璁璇璋璞璨璩璐璧瓒璺韪韫韬杌杓杞杈杩枥枇杪杳枘枧杵枨枞枭枋杷杼柰栉柘栊柩枰栌柙枵柚枳柝栀柃枸柢栎柁柽栲栳桠桡桎桢桄桤梃栝桕桦桁桧桀栾桊桉栩梵梏桴桷梓桫棂楮棼椟椠棹椤棰椋椁楗棣椐楱椹楠楂楝榄楫榀榘楸椴槌榇榈槎榉楦楣楹榛榧榻榫榭槔榱槁槊槟榕槠榍槿樯槭樗樘橥槲橄樾檠橐橛樵檎橹樽樨橘橼檑檐檩檗檫猷獒殁殂殇殄殒殓殍殚殛殡殪轫轭轱轲轳轵轶" +
      "轸轷轹轺轼轾辁辂辄辇辋辍辎辏辘辚軎戋戗戛戟戢戡戥戤戬臧瓯瓴瓿甏甑甓攴旮旯旰昊昙杲昃昕昀炅曷昝昴昱昶昵耆晟晔晁晏晖晡晗晷暄暌暧暝暾曛曜曦曩贲贳贶贻贽赀赅赆赈赉赇赍赕赙觇觊觋觌觎觏觐觑牮犟牝牦牯牾牿犄犋犍犏犒挈挲掰搿擘耄毪毳毽毵毹氅氇氆氍氕氘氙氚氡氩氤氪氲攵敕敫牍牒牖爰虢刖肟肜肓肼朊肽肱肫肭肴肷胧胨胩胪胛胂胄胙胍胗朐胝胫胱胴胭脍脎胲胼朕脒豚脶脞脬脘脲腈腌腓腴腙腚腱腠腩腼腽腭腧塍媵膈膂膑滕膣膪臌朦臊膻臁膦欤欷欹歃歆歙飑飒飓飕飙飚殳彀毂觳斐齑斓於旆旄旃旌旎旒旖炀炜炖炝炻烀炷炫炱烨烊焐焓焖焯焱" +
      "煳煜煨煅煲煊煸煺熘熳熵熨熠燠燔燧燹爝爨灬焘煦熹戾戽扃扈扉礻祀祆祉祛祜祓祚祢祗祠祯祧祺禅禊禚禧禳忑忐怼恝恚恧恁恙恣悫愆愍慝憩憝懋懑戆肀聿沓泶淼矶矸砀砉砗砘砑斫砭砜砝砹砺砻砟砼砥砬砣砩硎硭硖硗砦硐硇硌硪碛碓碚碇碜碡碣碲碹碥磔磙磉磬磲礅磴礓礤礞礴龛黹黻黼盱眄眍盹眇眈眚眢眙眭眦眵眸睐睑睇睃睚睨睢睥睿瞍睽瞀瞌瞑瞟瞠瞰瞵瞽町畀畎畋畈畛畲畹疃罘罡罟詈罨罴罱罹羁罾盍盥蠲钅钆钇钋钊钌钍钏钐钔钗钕钚钛钜钣钤钫钪钭钬钯钰钲钴钶钷钸钹钺钼钽钿铄铈铉铊铋铌铍铎铐铑铒铕铖铗铙铘铛铞铟铠铢铤铥铧铨铪铩铫铮铯铳铴铵铷铹铼" +
      "铽铿锃锂锆锇锉锊锍锎锏锒锓锔锕锖锘锛锝锞锟锢锪锫锩锬锱锲锴锶锷锸锼锾锿镂锵镄镅镆镉镌镎镏镒镓镔镖镗镘镙镛镞镟镝镡镢镤镥镦镧镨镩镪镫镬镯镱镲镳锺矧矬雉秕秭秣秫稆嵇稃稂稞稔稹稷穑黏馥穰皈皎皓皙皤瓞瓠甬鸠鸢鸨鸩鸪鸫鸬鸲鸱鸶鸸鸷鸹鸺鸾鹁鹂鹄鹆鹇鹈鹉鹋鹌鹎鹑鹕鹗鹚鹛鹜鹞鹣鹦鹧鹨鹩鹪鹫鹬鹱鹭鹳疒疔疖疠疝疬疣疳疴疸痄疱疰痃痂痖痍痣痨痦痤痫痧瘃痱痼痿瘐瘀瘅瘌瘗瘊瘥瘘瘕瘙瘛瘼瘢瘠癀瘭瘰瘿瘵癃瘾瘳癍癞癔癜癖癫癯翊竦穸穹窀窆窈窕窦窠窬窨窭窳衤衩衲衽衿袂裆袷袼裉裢裎裣裥裱褚裼裨裾裰褡褙褓褛褊褴褫褶襁襦疋胥皲皴矜耒" +
      "耔耖耜耠耢耥耦耧耩耨耱耋耵聃聆聍聒聩聱覃顸颀颃颉颌颍颏颔颚颛颞颟颡颢颥颦虍虔虬虮虿虺虼虻蚨蚍蚋蚬蚝蚧蚣蚪蚓蚩蚶蛄蚵蛎蚰蚺蚱蚯蛉蛏蚴蛩蛱蛲蛭蛳蛐蜓蛞蛴蛟蛘蛑蜃蜇蛸蜈蜊蜍蜉蜣蜻蜞蜥蜮蜚蜾蝈蜴蜱蜩蜷蜿螂蜢蝽蝾蝻蝠蝰蝌蝮螋蝓蝣蝼蝤蝙蝥螓螯螨蟒蟆螈螅螭螗螃螫蟥螬螵螳蟋蟓螽蟑蟀蟊蟛蟪蟠蟮蠖蠓蟾蠊蠛蠡蠹蠼缶罂罄罅舐竺竽笈笃笄笕笊笫笏筇笸笪笙笮笱笠笥笤笳笾笞筘筚筅筵筌筝筠筮筻筢筲筱箐箦箧箸箬箝箨箅箪箜箢箫箴篑篁篌篝篚篥篦篪簌篾篼簏簖簋簟簪簦簸籁籀臾舁舂舄臬衄舡舢舣舭舯舨舫舸舻舳舴舾艄艉艋艏艚艟艨衾袅袈裘裟襞羝羟" +
      "羧羯羰羲籼敉粑粝粜粞粢粲粼粽糁糇糌糍糈糅糗糨艮暨羿翎翕翥翡翦翩翮翳糸絷綦綮繇纛麸麴赳趄趔趑趱赧赭豇豉酊酐酎酏酤酢酡酰酩酯酽酾酲酴酹醌醅醐醍醑醢醣醪醭醮醯醵醴醺豕鹾趸跫踅蹙蹩趵趿趼趺跄跖跗跚跞跎跏跛跆跬跷跸跣跹跻跤踉跽踔踝踟踬踮踣踯踺蹀踹踵踽踱蹉蹁蹂蹑蹒蹊蹰蹶蹼蹯蹴躅躏躔躐躜躞豸貂貊貅貘貔斛觖觞觚觜觥觫觯訾謦靓雩雳雯霆霁霈霏霎霪霭霰霾龀龃龅龆龇龈龉龊龌黾鼋鼍隹隼隽雎雒瞿雠銎銮鋈錾鍪鏊鎏鐾鑫鱿鲂鲅鲆鲇鲈稣鲋鲎鲐鲑鲒鲔鲕鲚鲛鲞鲟鲠鲡鲢鲣鲥鲦鲧鲨鲩鲫鲭鲮鲰鲱鲲鲳鲴鲵鲶鲷鲺鲻鲼鲽鳄鳅鳆鳇鳊鳋鳌鳍鳎鳏鳐鳓鳔" + "鳕鳗鳘鳙鳜鳝鳟鳢靼鞅鞑鞒鞔鞯鞫鞣鞲鞴骱骰骷鹘骶骺骼髁髀髅髂髋髌髑魅魃魇魉魈魍魑飨餍餮饕饔髟髡髦髯髫髻髭髹鬈鬏鬓鬟鬣麽麾縻麂麇麈麋麒鏖麝麟黛黜黝黠黟黢黩黧黥黪黯鼢鼬鼯鼹鼷鼽鼾齄";
        byte[] array = new byte[2];
        string return_py = "";
        for (int i = 0; i < transName.Length; i++)
        {
            array = System.Text.Encoding.Default.GetBytes(transName[i].ToString());
            if (array[0] < 176)  //.非汉字  
            {
                return_py += transName[i];
            }
            else if (array[0] >= 176 && array[0] <= 215)  //一级汉字  
            {
                if (transName[i].ToString().CompareTo("匝") >= 0)
                    return_py += "z";
                else if (transName[i].ToString().CompareTo("压") >= 0)
                    return_py += "y";
                else if (transName[i].ToString().CompareTo("昔") >= 0)
                    return_py += "x";
                else if (transName[i].ToString().CompareTo("挖") >= 0)
                    return_py += "w";
                else if (transName[i].ToString().CompareTo("塌") >= 0)
                    return_py += "t";
                else if (transName[i].ToString().CompareTo("撒") >= 0)
                    return_py += "s";
                else if (transName[i].ToString().CompareTo("然") >= 0)
                    return_py += "r";
                else if (transName[i].ToString().CompareTo("期") >= 0)
                    return_py += "q";
                else if (transName[i].ToString().CompareTo("啪") >= 0)
                    return_py += "p";
                else if (transName[i].ToString().CompareTo("哦") >= 0)
                    return_py += "o";
                else if (transName[i].ToString().CompareTo("拿") >= 0)
                    return_py += "n";
                else if (transName[i].ToString().CompareTo("妈") >= 0)
                    return_py += "m";
                else if (transName[i].ToString().CompareTo("垃") >= 0)
                    return_py += "l";
                else if (transName[i].ToString().CompareTo("喀") >= 0)
                    return_py += "k";
                else if (transName[i].ToString().CompareTo("击") >= 0)
                    return_py += "j";
                else if (transName[i].ToString().CompareTo("哈") >= 0)
                    return_py += "h";
                else if (transName[i].ToString().CompareTo("噶") >= 0)
                    return_py += "g";
                else if (transName[i].ToString().CompareTo("发") >= 0)
                    return_py += "f";
                else if (transName[i].ToString().CompareTo("蛾") >= 0)
                    return_py += "e";
                else if (transName[i].ToString().CompareTo("搭") >= 0)
                    return_py += "d";
                else if (transName[i].ToString().CompareTo("擦") >= 0)
                    return_py += "c";
                else if (transName[i].ToString().CompareTo("芭") >= 0)
                    return_py += "b";
                else if (transName[i].ToString().CompareTo("啊") >= 0)
                    return_py += "a";
            }
            else if (array[0] >= 215)    //二级汉字  
            {
                return_py += ls_second_eng.Substring(ls_second_ch.IndexOf(transName[i].ToString(), 0), 1);
            }
        }
        return return_py.ToLower();
    }
}
#endregion ///////////////////////////////////

/// <summary>
/// SqlHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
/// ability to discover parameters for stored procedures at run-time.
/// </summary>
public sealed class SqlHelperParameterCache
{
    #region private methods, variables, and constructors

    //Since this class provides only static methods, make the default constructor private to prevent 
    //instances from being created with "new SqlHelperParameterCache()"
    private SqlHelperParameterCache() { }

    private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

    /// <summary>
    /// Resolve at run time the appropriate set of SqlParameters for a stored procedure
    /// </summary>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="includeReturnValueParameter">Whether or not to include their return value parameter</param>
    /// <returns>The parameter array discovered.</returns>
    private static SqlParameter[] DiscoverSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        SqlCommand cmd = new SqlCommand(spName, connection);
        cmd.CommandType = CommandType.StoredProcedure;

        connection.Open();
        SqlCommandBuilder.DeriveParameters(cmd);
        connection.Close();

        if (!includeReturnValueParameter)
        {
            cmd.Parameters.RemoveAt(0);
        }

        SqlParameter[] discoveredParameters = new SqlParameter[cmd.Parameters.Count];

        cmd.Parameters.CopyTo(discoveredParameters, 0);

        // Init the parameters with a DBNull value
        foreach (SqlParameter discoveredParameter in discoveredParameters)
        {
            discoveredParameter.Value = DBNull.Value;
        }
        return discoveredParameters;
    }

    /// <summary>
    /// Deep copy of cached SqlParameter array
    /// </summary>
    /// <param name="originalParameters"></param>
    /// <returns></returns>
    private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
    {
        SqlParameter[] clonedParameters = new SqlParameter[originalParameters.Length];

        for (int i = 0, j = originalParameters.Length; i < j; i++)
        {
            clonedParameters[i] = (SqlParameter)((ICloneable)originalParameters[i]).Clone();
        }

        return clonedParameters;
    }

    #endregion private methods, variables, and constructors

    #region caching functions

    /// <summary>
    /// Add parameter array to the cache
    /// </summary>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters to be cached</param>
    public static void CacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

        string hashKey = connectionString + ":" + commandText;

        paramCache[hashKey] = commandParameters;
    }

    /// <summary>
    /// Retrieve a parameter array from the cache
    /// </summary>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>An array of SqlParamters</returns>
    public static SqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

        string hashKey = connectionString + ":" + commandText;

        SqlParameter[] cachedParameters = paramCache[hashKey] as SqlParameter[];
        if (cachedParameters == null)
        {
            return null;
        }
        else
        {
            return CloneParameters(cachedParameters);
        }
    }

    #endregion caching functions

    #region Parameter Discovery Functions

    /// <summary>
    /// Retrieves the set of SqlParameters appropriate for the stored procedure
    /// </summary>
    /// <remarks>
    /// This method will query the database for this information, and then store it in a cache for future requests.
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <returns>An array of SqlParameters</returns>
    public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
    {
        return GetSpParameterSet(connectionString, spName, false);
    }

    /// <summary>
    /// Retrieves the set of SqlParameters appropriate for the stored procedure
    /// </summary>
    /// <remarks>
    /// This method will query the database for this information, and then store it in a cache for future requests.
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
    /// <returns>An array of SqlParameters</returns>
    public static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
    {
        if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
        }
    }

    /// <summary>
    /// Retrieves the set of SqlParameters appropriate for the stored procedure
    /// </summary>
    /// <remarks>
    /// This method will query the database for this information, and then store it in a cache for future requests.
    /// </remarks>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <returns>An array of SqlParameters</returns>
    internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName)
    {
        return GetSpParameterSet(connection, spName, false);
    }

    /// <summary>
    /// Retrieves the set of SqlParameters appropriate for the stored procedure
    /// </summary>
    /// <remarks>
    /// This method will query the database for this information, and then store it in a cache for future requests.
    /// </remarks>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
    /// <returns>An array of SqlParameters</returns>
    internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        using (SqlConnection clonedConnection = (SqlConnection)((ICloneable)connection).Clone())
        {
            return GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
        }
    }

    /// <summary>
    /// Retrieves the set of SqlParameters appropriate for the stored procedure
    /// </summary>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
    /// <returns>An array of SqlParameters</returns>
    private static SqlParameter[] GetSpParameterSetInternal(SqlConnection connection, string spName, bool includeReturnValueParameter)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

        SqlParameter[] cachedParameters;

        cachedParameters = paramCache[hashKey] as SqlParameter[];
        if (cachedParameters == null)
        {
            SqlParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
            paramCache[hashKey] = spParameters;
            cachedParameters = spParameters;
        }

        return CloneParameters(cachedParameters);
    }

    #endregion Parameter Discovery Functions

}
