#region Copyright & License
//
// Copyright 2001-2006 The Apache Software Foundation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Reflection;
using log4net;
using log4net.Core;

// This interface is designed to look like log4net's ILog interface.
// We have this as a facade in front of it to avoid crashing if the
// hosting web site chooses not to deploy log4net.dll along with
// dotnetopenid.dll.

namespace YOURLIBNAME.Loggers
{
	/// <summary>
	/// The ILog interface is use by application to log messages into
	/// the log4net framework.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use the <see cref="LogManager"/> to obtain logger instances
	/// that implement this interface. The <see cref="LogManager.GetLogger(Assembly,Type)"/>
	/// static method is used to get logger instances.
	/// </para>
	/// <para>
	/// This class contains methods for logging at different levels and also
	/// has properties for determining if those logging levels are
	/// enabled in the current configuration.
	/// </para>
	/// <para>
	/// This interface can be implemented in different ways. This documentation
	/// specifies reasonable behavior that a caller can expect from the actual
	/// implementation, however different implementations reserve the right to
	/// do things differently.
	/// </para>
	/// </remarks>
	/// <example>Simple example of logging messages
	/// <code lang="C#">
	/// ILog log = LogManager.GetLogger("application-log");
	/// 
	/// log.Info("Application Start");
	/// log.Debug("This is a debug message");
	/// 
	/// if (log.IsDebugEnabled)
	/// {
	///		log.Debug("This is another debug message");
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="LogManager"/>
	/// <seealso cref="LogManager.GetLogger(Assembly, Type)"/>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	interface ILog
	{
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
		void Debug(object message);
  
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
		void Debug(object message, Exception exception);

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
		void DebugFormat(string format, params object[] args); 

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
		void DebugFormat(string format, object arg0); 

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
		void DebugFormat(string format, object arg0, object arg1); 

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
		void DebugFormat(string format, object arg0, object arg1, object arg2); 

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
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
		void DebugFormat(IFormatProvider provider, string format, params object[] args);

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
		void Info(object message);
  
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
		void Info(object message, Exception exception);

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
		void InfoFormat(string format, params object[] args);

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
		void InfoFormat(string format, object arg0); 

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
		void InfoFormat(string format, object arg0, object arg1); 

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
		void InfoFormat(string format, object arg0, object arg1, object arg2); 

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
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
		void InfoFormat(IFormatProvider provider, string format, params object[] args);

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
		void Warn(object message);
  
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
		void Warn(object message, Exception exception);

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
		void WarnFormat(string format, params object[] args);

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
		void WarnFormat(string format, object arg0); 

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
		void WarnFormat(string format, object arg0, object arg1); 

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
		void WarnFormat(string format, object arg0, object arg1, object arg2); 

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
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
		void WarnFormat(IFormatProvider provider, string format, params object[] args);

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
		void Error(object message);

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
		void Error(object message, Exception exception);

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
		void ErrorFormat(string format, params object[] args);

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
		void ErrorFormat(string format, object arg0); 

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
		void ErrorFormat(string format, object arg0, object arg1); 

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
		void ErrorFormat(string format, object arg0, object arg1, object arg2); 

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
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
		void ErrorFormat(IFormatProvider provider, string format, params object[] args);

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
		void Fatal(object message);
  
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
		void Fatal(object message, Exception exception);

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
		void FatalFormat(string format, params object[] args);

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
		void FatalFormat(string format, object arg0); 

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
		void FatalFormat(string format, object arg0, object arg1); 

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
		void FatalFormat(string format, object arg0, object arg1, object arg2); 

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
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
		void FatalFormat(IFormatProvider provider, string format, params object[] args);

		/// <summary>
		/// Checks if this logger is enabled for the <see cref="Level.Debug"/> level.
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
		/// <seealso cref="DebugFormat(IFormatProvider, string, object[])"/>
		bool IsDebugEnabled { get; }
  
		/// <summary>
		/// Checks if this logger is enabled for the <see cref="Level.Info"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.Info"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// For more information see <see cref="ILog.IsDebugEnabled"/>.
		/// </remarks>
		/// <seealso cref="Info(object)"/>
		/// <seealso cref="InfoFormat(IFormatProvider, string, object[])"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		bool IsInfoEnabled { get; }

		/// <summary>
		/// Checks if this logger is enabled for the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.Warn"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// For more information see <see cref="ILog.IsDebugEnabled"/>.
		/// </remarks>
		/// <seealso cref="Warn(object)"/>
		/// <seealso cref="WarnFormat(IFormatProvider, string, object[])"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		bool IsWarnEnabled { get; }

		/// <summary>
		/// Checks if this logger is enabled for the <see cref="Level.Error"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.Error"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// For more information see <see cref="ILog.IsDebugEnabled"/>.
		/// </remarks>
		/// <seealso cref="Error(object)"/>
		/// <seealso cref="ErrorFormat(IFormatProvider, string, object[])"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		bool IsErrorEnabled { get; }

		/// <summary>
		/// Checks if this logger is enabled for the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <see cref="Level.Fatal"/> events, <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// For more information see <see cref="ILog.IsDebugEnabled"/>.
		/// </remarks>
		/// <seealso cref="Fatal(object)"/>
		/// <seealso cref="FatalFormat(IFormatProvider, string, object[])"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		bool IsFatalEnabled { get; }
	}
}
