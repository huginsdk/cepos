using System;
using System.IO;
using System.Windows.Forms;
using Hugin.POS.Common;
using System.Security.Principal;
using System.Reflection;

namespace Hugin.POS
{

    public delegate void FatalEventHandler(String DisplayMessage);

    public class Chassis
    {
        public static MainForm Engine = null;

        public static FatalEventHandler FatalErrorOccured;
#if !WindowsCE
        [STAThread]
#else
        [MTAThread]
#endif
        public static void Main(string[] args)
        {
            try
            {
#if !WindowsCE
                CloseRunningProgram();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Engine = new CashRegisterInput();
                Engine.Visible = false;
                Application.Run(Engine);
                Application.Exit();
#endif
            }
#if !WindowsCE
            catch (TypeInitializationException)
            {
                ShowError("POS.EXE.CONFIG, EKSÝK VEYA HATALI");
            }
#endif
            catch (UnauthorizedAccessException)
            {
                ShowError(PosMessage.PORT_IN_USE);
            }
            catch (FileNotFoundException ex)
            {
#if !WindowsCE
                if (ex.FileName.IndexOf(',') > 0)
                {
                    MessageBox.Show(PosMessage.MISSING_REFERENCE_FILE+" : " + ex.FileName.Substring(0, ex.FileName.IndexOf(',')));
                }
                else
                {
                    String msg = ex.Message;
                    if (msg.Length > 20)
                        msg = msg.Substring(0, 20);
                    ShowError(msg);
                }
#else

                ShowError(PosMessage.MISSING_REFERENCE_FILE);
#endif
            }
            catch (Exception ex)
            {
                String msg = ex.Message;
                if (msg.Length > 20)
                    msg = msg.Substring(0, 20);
                ShowError(msg);
                Debugger.Instance().AppendLine(ex.StackTrace);
            }
        }

        private static void ShowError(string message)
        {
            MessageBox.Show(PosMessage.CLOSE_PROGRAM + "\n" + message);
        }

        public static void RestartProgram(bool updateProgram)
        {
            String huginPOSPath = IOUtil.ProgramDirectory + IOUtil.AssemblyName;

            System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();

            System.Diagnostics.ProcessStartInfo psi;
            if (!File.Exists(huginPOSPath))
            {
                psi = new System.Diagnostics.ProcessStartInfo("ShutDown", "/t 1 /r");
                DisplayAdapter.Cashier.Show(PosMessage.RESTART_COMPUTER);
            }
            else
            {
                if (updateProgram)
                {
                    CreateBatchFile();
                    huginPOSPath = IOUtil.ProgramDirectory + "upgrade.bat";
                }

                psi = new System.Diagnostics.ProcessStartInfo(huginPOSPath, "");
                DisplayAdapter.Cashier.Show(PosMessage.RESTART_PROGRAM);
            }

            System.Diagnostics.Process processStarting = new System.Diagnostics.Process();
            processStarting.StartInfo = psi;

            processStarting.Start();
            currentProcess.Kill();
        }

        public static void CloseApplication()
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private static void CreateBatchFile()
        {
            try
            {
                var strBatch = "ping 127.0.0.1 /n 2\r\n";
                strBatch += string.Format("XCOPY {0}* {1} /s /i /y \r\n", PosConfiguration.UpgradePath.Replace("/", "\\"), PosConfiguration.UpgradePath.Replace("Upgrade/", ""));

                if (!File.Exists(PosConfiguration.UpgradePath + "service.txt"))
                {
                    if (CashRegister.IsDesktopWindows)
                        strBatch += String.Format("start {0}\n", IOUtil.ProgramDirectory.Replace('/', '\\') + IOUtil.AssemblyName);
                    else
                        strBatch += String.Format("start \"{0}\"\n", IOUtil.ProgramDirectory.Replace('/', '\\') + IOUtil.AssemblyName);
                }
                strBatch += "exit";

                IOUtil.WriteAllText(IOUtil.ProgramDirectory + "upgrade.bat", strBatch);
            }
            catch (Exception ex)
            {
                Debugger.Instance().AppendLine(ex.Message);
            }
        }

        private static void CloseRunningProgram()
        {
#if !WindowsCE
            System.Diagnostics.Process[] procNames = System.Diagnostics.Process.GetProcessesByName("pos");
            if (procNames.GetLength(0) > 0)
            {
                for (int i = 0; i < procNames.GetLength(0); i++)
                {
                    if (procNames[i].Id == System.Diagnostics.Process.GetCurrentProcess().Id) continue;
                    procNames[i].Kill();
                    procNames[i].WaitForExit();
                }
            }
#endif

        }

        internal static void ShutDown(bool restart)
        {
            System.Diagnostics.ProcessStartInfo psi;
            psi = new System.Diagnostics.ProcessStartInfo("ShutDown", restart ? "/t 1 /r" : "/p");

            System.Diagnostics.Process processStarting = new System.Diagnostics.Process();
            processStarting.StartInfo = psi;

            processStarting.Start();
        }
    }        

}

