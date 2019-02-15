using System;
using System.IO;

namespace Hugin.POS.Common
{
    /// <summary>
    /// An object that provides basic logging capabilities.
    /// </summary>
    public class EZLogger
    {        
        private static EZLogger logger;

        #region Attributes

        /// <summary>
        /// Log levels.
        /// </summary>
        public enum Level
        {
            /// <summary>Log debug messages.</summary>
            Debug = 1,

            /// <summary>Log informational messages.</summary>
            Info = 2,

            /// <summary>Log success messages.</summary>
            Success = 4,

            /// <summary>Log warning messages.</summary>
            Warning = 8,

            /// <summary>Log error messages.</summary>
            Error = 16,

            /// <summary>Log fatal errors.</summary>
            Fatal = 32,

            /// <summary>Log all messages.</summary>
            All = 0xFFFF,
        }

        /// <summary>
        /// The logger's state.
        /// </summary>
        public enum State
        {
            /// <summary>The logger is stopped.</summary>
            Stopped = 0,

            /// <summary>The logger has been started.</summary>
            Running,

            /// <summary>The logger is paused.</summary>
            Paused,
        }

        #endregion

        #region Construction/destruction

        /// <summary>
        /// Constructs a EZLogger.
        /// </summary>
        /// <param name="logFilename">Log file to receive output.</param>
        /// <param name="bAppend">Flag: append to existing file (if any).</param>
        /// <param name="logLevels">Mask indicating log levels of interest.</param>
        public EZLogger
          (string logFilename,
           bool bAppend,
           uint logLevels)
        {
            //to do:
            //States.ReportMenu.OnZReportComplete += new ZEventHandler(zReport_OnComplete);
            _logFilename = logFilename;
            _bAppend = bAppend;
            _levels = logLevels;
        }

        public static EZLogger Log
        {
            get { return logger; }
            set { logger = value; }
        }

        /// <summary>
        /// Private default constructor.
        /// </summary>
        private EZLogger()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets name of the log file.
        /// </summary>
        public string LogFileName
        {
            get { return _logFilename; }
        }


        /// <summary>
        /// Gets and sets the log level.
        /// </summary>
        public uint Levels
        {
            get { return _levels; }
            set { _levels = value; }
        }

        /// <summary>
        /// Retrieves the logger's state.
        /// </summary>
        public State LoggerState
        {
            get { return _state; }
        }

        #endregion

        #region Operations

        /// <summary>
        /// Starts logging.
        /// </summary>
        /// <returns>true if successful, false otherwise.</returns>
        public bool Start()
        {
            lock (this)
            {
                // Fail if logging has already been started
                if (LoggerState != State.Stopped)
                    return false;

                // Fail if the log file isn't specified
                if (String.IsNullOrEmpty(_logFilename))
                    return false;

                // Delete log file if it exists
                if (!_bAppend)
                {
                    try
                    {
                        File.Delete(_logFilename);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                // Open file for writing - return on error
                try
                {
                    _logFile = new StreamWriter(_logFilename, true, PosConfiguration.DefaultEncoding);
                }
                catch (Exception)
                {
                    _logFile = null;
                    return false;
                }

                _logFile.AutoFlush = true;

                // Return successfully
                _state = EZLogger.State.Running;
                return true;
            }
        }

        /// <summary>
        /// Temporarily suspends logging.
        /// </summary>
        /// <returns>true if successful, false otherwise.</returns>
        public bool Pause()
        {
            lock (this)
            {
                // Fail if logging hasn't been started
                if (LoggerState != State.Running)
                    return false;

                // Pause the logger
                _state = EZLogger.State.Paused;
                return true;
            }
        }

        /// <summary>
        /// Resumes logging.
        /// </summary>
        /// <returns>true if successful, false otherwise.</returns>
        public bool Resume()
        {
            lock (this)
            {
                // Fail if logging hasn't been paused
                if (LoggerState != State.Paused)
                    return false;

                // Resume logging
                _state = EZLogger.State.Running;
                return true;
            }
        }

        /// <summary>
        /// Stops logging.
        /// </summary>
        /// <returns>true if successful, false otherwise.</returns>
        public bool Stop()
        {
            lock (this)
            {
                // Fail if logging hasn't been started
                if (LoggerState != State.Running)
                    return false;

                // Stop logging
                try
                {
                    _logFile.Close();
                    _logFile = null;
                }
                catch (Exception)
                {
                    return false;
                }
                _state = EZLogger.State.Stopped;
                return true;
            }
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns>true if successful, false otherwise.</returns>
        public bool Debug
          (string msg)
        {
            return WriteLogMsg(Level.Debug, msg);
        }
        public bool Debug
           (string format, params object[] args)
        {
            return WriteLogMsg(Level.Debug, String.Format(format, args));
        }
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns>true if successful, false otherwise.</returns>
        public bool Info
          (string msg)
        {
            return WriteLogMsg(Level.Info, msg);
        }

        public bool Info
           (string format, params object[] args)
        {
            return WriteLogMsg(Level.Info, String.Format(format, args));
        }

        /// <summary>
        /// Logs a success message.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns>true if successful, false otherwise.</returns>
        public bool Success
          (string msg)
        {
            return WriteLogMsg(Level.Success, msg);
        }

        public bool Success
           (string format, params object[] args)
        {
            return WriteLogMsg(Level.Success, String.Format(format, args));
        }
        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns>true if successful, false otherwise.</returns>
        public bool Warning
          (string msg)
        {
            return WriteLogMsg(Level.Warning, msg);
        }
        public bool Warning
           (string format, params object[] args)
        {
            return WriteLogMsg(Level.Warning, String.Format(format, args));
        }
        public bool Warning
        (Exception e)
        {
            return WriteLogMsg(Level.Warning, e);
        }
        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns>true if successful, false otherwise.</returns>
        public bool Error
          (string msg)
        {
            return WriteLogMsg(Level.Error, msg);
        }
        public bool Error
           (string format, params object[] args)
        {
            return WriteLogMsg(Level.Error, String.Format(format, args));
        }

        public bool Error
        (Exception e)
        {
            FileInfo logFile = new FileInfo(_logFilename);

            return WriteLogMsg(Level.Error, e);
        }
        /// <summary>
        /// Logs a fatal error message.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <returns>true if successful, false otherwise.</returns>
        public bool Fatal
          (string msg)
        {
            return WriteLogMsg(Level.Fatal, msg);
        }
        public bool Fatal
           (string format, params object[] args)
        {
            return WriteLogMsg(Level.Fatal, String.Format(format, args));
        }
        public bool Fatal
        (Exception e)
        {
            return WriteLogMsg(Level.Fatal, e);
        }
        #endregion

        #region Helper methods

        /// <summary>
        /// Writes a log message.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool WriteLogMsg
          (Level level,
           string msg)
        {
            lock (this)
            {
                try
                {
                    // Fail if logger hasn't been started
                    if (LoggerState == State.Stopped)
                        return false;

                    // Ignore message logging is paused or it doesn't pass the filter
                    if ((LoggerState == State.Paused) || ((_levels & (uint)level) != (uint)level))
                        return true;

                    //Weird way to mark files as "contains error or fatal messages"
                    //Would have used archive flag instead of NotContentIndexed but it keeps getting changed by C# file libraries
#if !WindowsCE
                    if ((level & Level.Error) == Level.Error || (level & Level.Fatal) == Level.Fatal)
                        File.SetAttributes(_logFilename, File.GetAttributes(_logFilename) | FileAttributes.NotContentIndexed);
#endif

                    // Write log message
                    DateTime tmNow = DateTime.Now;
                    string logMsg = String.Format("{0} {1}  {2}: {3}",
                                                   tmNow.ToShortDateString(), tmNow.ToLongTimeString(),
                                                   level.ToString().Substring(0, 1), msg.Replace('\n', '·'));
                    _logFile.WriteLine(logMsg);
                }
                catch (IOException)
                {
                    RecoverLogFile();
                }
                return true;
            }
        }

        private void RecoverLogFile()
        {
            try
            {
                string name = String.Format("{0}{0}{0}", Path.GetDirectoryName(LogFileName), DateTime.Now, Path.GetFileName(LogFileName));
                File.Move(LogFileName, name);
            }
            catch
            {

            }
        }

        private bool WriteLogMsg(Level level, Exception e)
        {
            lock (this)
            {

                // Fail if logger hasn't been started
                if (LoggerState == State.Stopped)
                    return false;

                // Ignore message logging is paused or it doesn't pass the filter
                if ((LoggerState == State.Paused) || ((_levels & (uint)level) != (uint)level))
                    return true;

                // Write log message
                WriteLogMsg(level, String.Format("{0} occured. {1}", e.GetType(), e.Message));

#if WindowsCE
                if (e is PosException)
                {
                    PosException pe = (PosException)e;

                    foreach (String key in pe.Data.Keys)
                        WriteLogMsg(level, String.Format("{0}: {1}", key, pe.Data[key]));
                }
#else
                foreach (Object o in e.Data.Keys)
                    WriteLogMsg(level, String.Format("{0}: {1}",o,e.Data[o]));
#endif
                if (e.StackTrace != null)
                    foreach (String stackTraceLine in e.StackTrace.Split('\r'))
                        WriteLogMsg(level, stackTraceLine);
                return true;
            }
        }

        #endregion

        #region Fields

        /// <summary>Name of the log file.</summary>
        private string _logFilename;

        /// <summary>Flag: append to existing file (if any).</summary>
        private bool _bAppend = true;

        /// <summary>The log file.</summary>
        private StreamWriter _logFile = null;

        /// <summary>Levels to be logged.</summary>
        private uint _levels = (uint)(Level.Warning | Level.Error | Level.Fatal);

        /// <summary>The logger's state.</summary>
        private State _state = State.Stopped;

        #endregion
    }
}
