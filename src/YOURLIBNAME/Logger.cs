//-----------------------------------------------------------------------
// <copyright file="Logger.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace YOURLIBNAME {
	using System;
	using System.Globalization;
	using log4net.Core;
	using YOURLIBNAME.Loggers;

	/// <summary>
	/// A general logger for the entire YOURLIBNAME library.
	/// </summary>
	/// <remarks>
	/// Because this logger is intended for use with non-localized strings, the
	/// overloads that take <see cref="CultureInfo"/> have been removed, and 
	/// <see cref="CultureInfo.InvariantCulture"/> is used implicitly.
	/// </remarks>
	internal static class Logger {
		/// <summary>
		/// The <see cref="ILog"/> instance that is to be used 
		/// by this static Logger for the duration of the appdomain.
		/// </summary>
		private static ILog facade = Create("YOURLIBNAME");

		#region ILog Members
		//// Although this static class doesn't literally implement the ILog interface, 
		//// we implement (mostly) all the same methods in a static way.

		/// <summary>
		/// Gets a value indicating whether this logger is enabled for the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.Debug"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// <para>
		/// This function is intended to lessen the computational cost of
		/// disabled log debug statements.
		/// </para>
		/// <para> For some ILog interface <c>log</c>, when you write:</para>
		/// <code lang="C#">
		/// log.Debug("This is entry number: " + i );
		/// </code>
		/// <para>
		/// You incur the cost constructing the message, string construction and concatenation in
		/// this case, regardless of whether the message is logged or not.
		/// </para>
		/// <para>
		/// If you are worried about speed (who isn't), then you should write:
		/// </para>
		/// <code lang="C#">
		/// if (log.IsDebugEnabled)
		/// { 
		///     log.Debug("This is entry number: " + i );
		/// }
		/// </code>
		/// <para>
		/// This way you will not incur the cost of parameter
		/// construction if debugging is disabled for <c>log</c>. On
		/// the other hand, if the <c>log</c> is debug enabled, you
		/// will incur the cost of evaluating whether the logger is debug
		/// enabled twice. Once in <see cref="IsDebugEnabled"/> and once in
		/// the <see cref="Debug(object)"/>.  This is an insignificant overhead
		/// since evaluating a logger takes about 1% of the time it
		/// takes to actually log. This is the preferred style of logging.
		/// </para>
		/// <para>Alternatively if your logger is available statically then the is debug
		/// enabled state can be stored in a static variable like this:
		/// </para>
		/// <code lang="C#">
		/// private static readonly bool isDebugEnabled = log.IsDebugEnabled;
		/// </code>
		/// <para>
		/// Then when you come to log you can write:
		/// </para>
		/// <code lang="C#">
		/// if (isDebugEnabled)
		/// { 
		///     log.Debug("This is entry number: " + i );
		/// }
		/// </code>
		/// <para>
		/// This way the debug enabled state is only queried once
		/// when the class is loaded. Using a <c>private static readonly</c>
		/// variable is the most efficient because it is a run time constant
		/// and can be heavily optimized by the JIT compiler.
		/// </para>
		/// <para>
		/// Of course if you use a static readonly variable to
		/// hold the enabled state of the logger then you cannot
		/// change the enabled state at runtime to vary the logging
		/// that is produced. You have to decide if you need absolute
		/// speed or runtime flexibility.
		/// </para>
		/// </remarks>
		/// <seealso cref="Debug(object)"/>
		/// <seealso cref="DebugFormat(string, object[])"/>
		public static bool IsDebugEnabled {
			get { return facade.IsDebugEnabled; }
		}

		/// <summary>
		/// Gets a value indicating whether this logger is enabled for the <see cref="Level.Info"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.Info"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// For more information see <see cref="ILog.IsDebugEnabled"/>.
		/// </remarks>
		/// <seealso cref="Info(object)"/>
		/// <seealso cref="InfoFormat(string, object[])"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static bool IsInfoEnabled {
			get { return facade.IsInfoEnabled; }
		}

		/// <summary>
		/// Gets a value indicating whether this logger is enabled for the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.Warn"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// For more information see <see cref="ILog.IsDebugEnabled"/>.
		/// </remarks>
		/// <seealso cref="Warn(object)"/>
		/// <seealso cref="WarnFormat(string, object[])"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static bool IsWarnEnabled {
			get { return facade.IsWarnEnabled; }
		}

		/// <summary>
		/// Gets a value indicating whether this logger is enabled for the <see cref="Level.Error"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.Error"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// For more information see <see cref="ILog.IsDebugEnabled"/>.
		/// </remarks>
		/// <seealso cref="Error(object)"/>
		/// <seealso cref="ErrorFormat(string, object[])"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static bool IsErrorEnabled {
			get { return facade.IsErrorEnabled; }
		}

		/// <summary>
		/// Gets a value indicating whether this logger is enabled for the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.Fatal"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// For more information see <see cref="ILog.IsDebugEnabled"/>.
		/// </remarks>
		/// <seealso cref="Fatal(object)"/>
		/// <seealso cref="FatalFormat(string, object[])"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static bool IsFatalEnabled {
			get { return facade.IsFatalEnabled; }
		}

		/// <overloads>Log a message object with the <see cref="Level.Debug"/> level.</overloads>
		/// <summary>
		/// Log a message object with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>DEBUG</c>
		/// enabled by comparing the level of this logger with the 
		/// <see cref="Level.Debug"/> level. If this logger is
		/// <c>DEBUG</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="Debug(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Debug(object,Exception)"/>
		/// <seealso cref="IsDebugEnabled"/>
		public static void Debug(object message) {
			facade.Debug(message);
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Debug"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="Debug(object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="Debug(object)"/>
		/// <seealso cref="IsDebugEnabled"/>
		public static void Debug(object message, Exception exception) {
			facade.Debug(message, exception);
		}

		/// <overloads>Log a formatted string with the <see cref="Level.Debug"/> level.</overloads>
		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Debug(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Debug(object)"/>
		/// <seealso cref="IsDebugEnabled"/>
		public static void DebugFormat(string format, params object[] args) {
			facade.DebugFormat(CultureInfo.InvariantCulture, format, args);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Debug(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Debug(object)"/>
		/// <seealso cref="IsDebugEnabled"/>
		public static void DebugFormat(string format, object arg0) {
			facade.DebugFormat(format, arg0);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Debug(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Debug(object)"/>
		/// <seealso cref="IsDebugEnabled"/>
		public static void DebugFormat(string format, object arg0, object arg1) {
			facade.DebugFormat(format, arg0, arg1);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Debug(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Debug(object)"/>
		/// <seealso cref="IsDebugEnabled"/>
		public static void DebugFormat(string format, object arg0, object arg1, object arg2) {
			facade.DebugFormat(format, arg0, arg1, arg2);
		}

		/*
		public static void DebugFormat(IFormatProvider provider, string format, params object[] args) {
			facade.DebugFormat(provider, format, args);
		}
		*/

		/// <overloads>Log a message object with the <see cref="Level.Info"/> level.</overloads>
		/// <summary>
		/// Logs a message object with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>INFO</c>
		/// enabled by comparing the level of this logger with the 
		/// <see cref="Level.Info"/> level. If this logger is
		/// <c>INFO</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of the 
		/// additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="Info(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <seealso cref="Info(object,Exception)"/>
		/// <seealso cref="IsInfoEnabled"/>
		public static void Info(object message) {
			facade.Info(message);
		}

		/// <summary>
		/// Logs a message object with the <c>INFO</c> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="Info(object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="Info(object)"/>
		/// <seealso cref="IsInfoEnabled"/>
		public static void Info(object message, Exception exception) {
			facade.Info(message, exception);
		}

		/// <overloads>Log a formatted message string with the <see cref="Level.Info"/> level.</overloads>
		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Info(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Info(object,Exception)"/>
		/// <seealso cref="IsInfoEnabled"/>
		public static void InfoFormat(string format, params object[] args) {
			facade.InfoFormat(CultureInfo.InvariantCulture, format, args);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Info(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Info(object)"/>
		/// <seealso cref="IsInfoEnabled"/>
		public static void InfoFormat(string format, object arg0) {
			facade.InfoFormat(format, arg0);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Info(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Info(object)"/>
		/// <seealso cref="IsInfoEnabled"/>
		public static void InfoFormat(string format, object arg0, object arg1) {
			facade.InfoFormat(format, arg0, arg1);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Info(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Info(object)"/>
		/// <seealso cref="IsInfoEnabled"/>
		public static void InfoFormat(string format, object arg0, object arg1, object arg2) {
			facade.InfoFormat(format, arg0, arg1, arg2);
		}

		/*
		public static void InfoFormat(IFormatProvider provider, string format, params object[] args) {
			facade.InfoFormat(provider, format, args);
		}
		*/

		/// <overloads>Log a message object with the <see cref="Level.Warn"/> level.</overloads>
		/// <summary>
		/// Log a message object with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>WARN</c>
		/// enabled by comparing the level of this logger with the 
		/// <see cref="Level.Warn"/> level. If this logger is
		/// <c>WARN</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of the 
		/// additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="Warn(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <seealso cref="Warn(object,Exception)"/>
		/// <seealso cref="IsWarnEnabled"/>
		public static void Warn(object message) {
			facade.Warn(message);
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Warn"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="Warn(object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="Warn(object)"/>
		/// <seealso cref="IsWarnEnabled"/>
		public static void Warn(object message, Exception exception) {
			facade.Warn(message, exception);
		}

		/// <overloads>Log a formatted message string with the <see cref="Level.Warn"/> level.</overloads>
		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Warn(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Warn(object,Exception)"/>
		/// <seealso cref="IsWarnEnabled"/>
		public static void WarnFormat(string format, params object[] args) {
			facade.WarnFormat(CultureInfo.InvariantCulture, format, args);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Warn(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Warn(object)"/>
		/// <seealso cref="IsWarnEnabled"/>
		public static void WarnFormat(string format, object arg0) {
			facade.WarnFormat(format, arg0);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Warn(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Warn(object)"/>
		/// <seealso cref="IsWarnEnabled"/>
		public static void WarnFormat(string format, object arg0, object arg1) {
			facade.WarnFormat(format, arg0, arg1);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Warn(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Warn(object)"/>
		/// <seealso cref="IsWarnEnabled"/>
		public static void WarnFormat(string format, object arg0, object arg1, object arg2) {
			facade.WarnFormat(format, arg0, arg1, arg2);
		}

		/*
		public static void WarnFormat(IFormatProvider provider, string format, params object[] args) {
			facade.WarnFormat(provider, format, args);
		}
		*/

		/// <overloads>Log a message object with the <see cref="Level.Error"/> level.</overloads>
		/// <summary>
		/// Logs a message object with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>ERROR</c>
		/// enabled by comparing the level of this logger with the 
		/// <see cref="Level.Error"/> level. If this logger is
		/// <c>ERROR</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of the 
		/// additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="Error(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Error(object,Exception)"/>
		/// <seealso cref="IsErrorEnabled"/>
		public static void Error(object message) {
			facade.Error(message);
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Error"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="Error(object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="Error(object)"/>
		/// <seealso cref="IsErrorEnabled"/>
		public static void Error(object message, Exception exception) {
			facade.Error(message, exception);
		}

		/// <overloads>Log a formatted message string with the <see cref="Level.Error"/> level.</overloads>
		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Error(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Error(object,Exception)"/>
		/// <seealso cref="IsErrorEnabled"/>
		public static void ErrorFormat(string format, params object[] args) {
			facade.ErrorFormat(CultureInfo.InvariantCulture, format, args);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Error(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Error(object)"/>
		/// <seealso cref="IsErrorEnabled"/>
		public static void ErrorFormat(string format, object arg0) {
			facade.ErrorFormat(format, arg0);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Error(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Error(object)"/>
		/// <seealso cref="IsErrorEnabled"/>
		public static void ErrorFormat(string format, object arg0, object arg1) {
			facade.ErrorFormat(format, arg0, arg1);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Error(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Error(object)"/>
		/// <seealso cref="IsErrorEnabled"/>
		public static void ErrorFormat(string format, object arg0, object arg1, object arg2) {
			facade.ErrorFormat(format, arg0, arg1, arg2);
		}

		/*
		public static void ErrorFormat(IFormatProvider provider, string format, params object[] args) {
			facade.ErrorFormat(provider, format, args);
		}
		*/

		/// <overloads>Log a message object with the <see cref="Level.Fatal"/> level.</overloads>
		/// <summary>
		/// Log a message object with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>FATAL</c>
		/// enabled by comparing the level of this logger with the 
		/// <see cref="Level.Fatal"/> level. If this logger is
		/// <c>FATAL</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of the 
		/// additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="Fatal(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <param name="message">The message object to log.</param>
		/// <seealso cref="Fatal(object,Exception)"/>
		/// <seealso cref="IsFatalEnabled"/>
		public static void Fatal(object message) {
			facade.Fatal(message);
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Fatal"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="Fatal(object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="Fatal(object)"/>
		/// <seealso cref="IsFatalEnabled"/>
		public static void Fatal(object message, Exception exception) {
			facade.Fatal(message, exception);
		}

		/// <overloads>Log a formatted message string with the <see cref="Level.Fatal"/> level.</overloads>
		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Fatal(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Fatal(object,Exception)"/>
		/// <seealso cref="IsFatalEnabled"/>
		public static void FatalFormat(string format, params object[] args) {
			facade.FatalFormat(CultureInfo.InvariantCulture, format, args);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Fatal(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Fatal(object)"/>
		/// <seealso cref="IsFatalEnabled"/>
		public static void FatalFormat(string format, object arg0) {
			facade.FatalFormat(format, arg0);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Fatal(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Fatal(object)"/>
		/// <seealso cref="IsFatalEnabled"/>
		public static void FatalFormat(string format, object arg0, object arg1) {
			facade.FatalFormat(format, arg0, arg1);
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="Fatal(object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="Fatal(object)"/>
		/// <seealso cref="IsFatalEnabled"/>
		public static void FatalFormat(string format, object arg0, object arg1, object arg2) {
			facade.FatalFormat(format, arg0, arg1, arg2);
		}

		/*
		public static void FatalFormat(IFormatProvider provider, string format, params object[] args) {
			facade.FatalFormat(provider, format, args);
		}
		*/

		#endregion

		/// <summary>
		/// Creates an additional logger on demand for a subsection of the application.
		/// </summary>
		/// <param name="name">A name that will be included in the log file.</param>
		/// <returns>The <see cref="ILog"/> instance created with the given name.</returns>
		internal static ILog Create(string name) {
			if (String.IsNullOrEmpty(name)) {
				throw new ArgumentNullException("name");
			}

			return InitializeFacade(name);
		}

		/// <summary>
		/// Creates an additional logger on demand for a subsection of the application.
		/// </summary>
		/// <param name="type">A type whose full name that will be included in the log file.</param>
		/// <returns>The <see cref="ILog"/> instance created with the given type name.</returns>
		internal static ILog Create(Type type) {
			if (type == null) {
				throw new ArgumentNullException("type");
			}

			return Create(type.FullName);
		}

		/// <summary>
		/// Discovers the presence of Log4net.dll and other logging mechanisms
		/// and returns the best available logger.
		/// </summary>
		/// <param name="name">The name of the log to initialize.</param>
		/// <returns>The <see cref="ILog"/> instance of the logger to use.</returns>
		private static ILog InitializeFacade(string name) {
			ILog result = Log4NetLogger.Initialize(name) ?? TraceLogger.Initialize(name) ?? NoOpLogger.Initialize();
			result.Info(Util.LibraryVersion);
			return result;
		}
	}
}
