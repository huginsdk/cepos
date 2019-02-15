using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Hugin.POS.Common;
using System.Threading;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS
{
    class CashRegisterInput : MainForm, IDisplay
    {
        private MyButton btnPrgReport;
        private MyButton btnSaleReport;
        private MyButton btnEJDetail;
        private MyButton btnXReport;
        private MyButton btnZReport;
        private NumericUpDown numerFMEndZ;
        private NumericUpDown numerFMStartZ;
        private System.Windows.Forms.Label lblFMEndZ;
        private System.Windows.Forms.Label lblFMStartZ;
        private MyButton btnFMBwZ;
        private MyButton btnFMBwDates;
        private DateTimePicker dtFMEndDate;
        private DateTimePicker dtFMStartDate;
        private System.Windows.Forms.Label lblFMEndDate;
        private System.Windows.Forms.Label lblFMStartDate;
        private NumericUpDown numerZCopyZNo;
        private System.Windows.Forms.Label lblZCopyZNo;
        private MyButton btnZCopy;
        private NumericUpDown numerSingleCopyRcptNo;
        private NumericUpDown numerSingleCopyZNo;
        private System.Windows.Forms.Label lblSingleCopyRcptNo;
        private System.Windows.Forms.Label lblSingleCopyZNo;
        private MyButton btnDocCopy;
        private MyButton btnDateCopy;
        private DateTimePicker dtSingleCopyDate;
        private System.Windows.Forms.Label lblSingleCopyTime;
        private System.Windows.Forms.Label lblSingleCopyDate;
        private NumericUpDown numerPerCopyEndRcpt;
        private System.Windows.Forms.Label lblPerCopyEndRcpt;
        private NumericUpDown numerPerCopyStartRcpt;
        private System.Windows.Forms.Label lblPerCopyStartRcpt;
        private NumericUpDown numerPerCopyEndZ;
        private NumericUpDown numerPerCopyStartZ;
        private System.Windows.Forms.Label lblPerCopyEndZ;
        private System.Windows.Forms.Label lblPerCopyStartZ;
        private MyButton btnPerZR;
        private MyButton btnPerCopyDates;
        private DateTimePicker dtPerCopyEndDate;
        private DateTimePicker dtPerCopyStartDate;
        private System.Windows.Forms.Label lblPerCopyEndDate;
        private System.Windows.Forms.Label lblPerCopyStartDate;
        private System.Windows.Forms.Label lblPerCopyEndTime;
        private System.Windows.Forms.Label lblPerCopyStartTime;
        private System.Windows.Forms.Timer timerMain;
        private System.ComponentModel.IContainer components;
        private TimePicker tmPerCopyStartTime;
        private TimePicker tmPerCopyEndTime;
        private TimePicker tmSingleCopyTime;
        private Panel pnlMain;
        private MyButton btnReturnSale;
        private MyButton btnEJPeriodic;
        private MyButton btnEJSingle;
        private MyButton btnFMDate;
        private MyButton btnFMZZ;
        private Panel pnlFMZZ;
        private MyButton btnMainZZ;
        private MyButton btnRetSaleZZ;
        private Panel pnlFMDate;
        private MyButton btnMainFMDate;
        private MyButton btnRetSaleFMDate;
        private Panel pnlEJSingle;
        private MyButton btnEJSingDate;
        private MyButton btnEJSingByZR;
        private MyButton btnEJZCopy;
        private MyButton btnMainEJSing;
        private MyButton btnRetSaleEJSing;
        private Panel pnlEJPeriodic;
        private MyButton btnEJPerDaily;
        private MyButton btnEJPerDate;
        private MyButton btnEJPerZR;
        private MyButton btnMainEJPer;
        private MyButton btnRetSaleEJPer;
        private Panel pnlEJZCopy;
        private MyButton btnMainEJZCopy;
        private MyButton btnRetSaleEJZCopy;
        private Panel pnlEJSingDate;
        private MyButton btnMainEJSingDate;
        private MyButton btnRetSaleEJSingDate;
        private Panel pnlEJSingZR;
        private MyButton btnMainEJSingZR;
        private MyButton btnRetSaleEJSingZR;
        private Panel pnlEJPerDaily;
        private MyButton btnMainEJPerDaily;
        private MyButton btnRetSaleEJPerDaily;
        private Panel pnlEJPerDate;
        private MyButton btnMainEJPerDate;
        private MyButton btnRetSaleEJPerDate;
        private Panel pnlEJPerZR;
        private MyButton btnMainEJPerZR;
        private MyButton btnRetSaleZR;
        private MyButton btnEJDaily;
        private DateTimePicker dtEJPerDailyDate;
        private System.Windows.Forms.Label lblEJPerDailyDate;


        #region TO SATISFY OTHER "CashRegisterInput" REQUIREMENTS

        internal static DateTime lastKeyPressed = DateTime.Now;
        internal static event OnMessageHandler BarcodeReaded;
        internal static void ConnectBarcode()
        {

        }

        #endregion

        private static ErrForm errorForm = null;
        private static bool ready = false;
        private TcpService tcpService;

        private void ShowForm()
        {
            this.timerMain.Stop();
            this.Opacity = 1;
            this.CenterToScreen();
            this.TopMost = true;
            
            cr.State = States.ReportMenu.Instance();
        }

        private void HideForm()
        {
            this.timerMain.Start();
            this.Opacity = 0;

            try
            {
                cr.State = States.Start.Instance();
            }
            catch(Exception ex)
            {

            }
        }

        public CashRegisterInput()
        {
            InitializeComponent();
            errorForm = new ErrForm();

            timerMain.Start();
            this.ShowInTaskbar = false;
        }

        void Printer_DocumentRequested(object sender, EventArgs e)
        {
            errorForm.Show("BELGE KOYUP\nÇIKIŞ'A BASINIZ");
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!ready)
                {
                    this.PreProcess();

                    CheckBlocking();
                    ///now system is not idle for backgroundworker
                    cr.State = States.Start.Instance();

                    tcpService = new TcpService();

                    CashRegister.Printer.DocumentRequested += new EventHandler(Printer_DocumentRequested);

                    ready = true;

                    return;
                }

                if (this.Opacity == 0)
                {
                    CheckBlocking();

                    TCPMsgType tcpMsg = tcpService.Play();
                    if (tcpMsg == TCPMsgType.TCP_MSG_REPORT)
                    {
                        this.ShowForm();
                    }
                    if(tcpMsg == TCPMsgType.TCP_MSG_EXIT)
                    {
                        this.Close();
                    }
                }

            }
            catch
            {

            }
        }

        private void CheckBlocking()
        {
            //todo: search if timer stop is required in timer
            if (cr.State is States.BlockingState)
            {
                timerMain.Stop();
                while (cr.State is States.BlockingState)
                {
                    errorForm.Show(lastMsg);
                    cr.State.Escape();
                }
                timerMain.Start();
            }
            else if (Error.LastException != null)
            {
                timerMain.Stop();
                if (!(Error.LastException is PosException))
                {
                    Error er = new Error(Error.LastException);
                    errorForm.Show(er.Message);
                }
                Error.ResetLastException();
                timerMain.Start();

            }
        }

        private void SetReportsOnly(bool isEJOnly)
        {
            this.btnXReport.Enabled = !isEJOnly;
            this.btnZReport.Enabled = !isEJOnly;
            this.btnSaleReport.Enabled = !isEJOnly; 
            this.btnPrgReport.Enabled = !isEJOnly;
            this.btnFMBwDates.Enabled = !isEJOnly;
            this.btnFMBwZ.Enabled = !isEJOnly;
        }

        protected override void PreProcess()
        {
            try
            {
                CashRegister.Instance();

                Thread thread = new Thread(new ThreadStart(BackgroundWorker.Start));
                thread.Name = "BackgroundWorker";
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.BelowNormal;
                thread.Start();
            }
            catch (UnauthorizedAccessException ex)
            {
                throw ex;
            }
        }
        public override void Process(PosKey key)
        {
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CashRegisterInput));
            this.pnlEJPerDaily = new System.Windows.Forms.Panel();
            this.btnEJDaily = new Hugin.POS.MyButton();
            this.dtEJPerDailyDate = new System.Windows.Forms.DateTimePicker();
            this.lblEJPerDailyDate = new System.Windows.Forms.Label();
            this.btnMainEJPerDaily = new Hugin.POS.MyButton();
            this.btnRetSaleEJPerDaily = new Hugin.POS.MyButton();
            this.numerZCopyZNo = new System.Windows.Forms.NumericUpDown();
            this.lblZCopyZNo = new System.Windows.Forms.Label();
            this.btnZCopy = new Hugin.POS.MyButton();
            this.numerSingleCopyRcptNo = new System.Windows.Forms.NumericUpDown();
            this.numerSingleCopyZNo = new System.Windows.Forms.NumericUpDown();
            this.lblSingleCopyRcptNo = new System.Windows.Forms.Label();
            this.lblSingleCopyZNo = new System.Windows.Forms.Label();
            this.btnDocCopy = new Hugin.POS.MyButton();
            this.tmSingleCopyTime = new Hugin.POS.TimePicker(this.components);
            this.btnDateCopy = new Hugin.POS.MyButton();
            this.dtSingleCopyDate = new System.Windows.Forms.DateTimePicker();
            this.lblSingleCopyTime = new System.Windows.Forms.Label();
            this.lblSingleCopyDate = new System.Windows.Forms.Label();
            this.numerPerCopyEndRcpt = new System.Windows.Forms.NumericUpDown();
            this.lblPerCopyEndRcpt = new System.Windows.Forms.Label();
            this.numerPerCopyStartRcpt = new System.Windows.Forms.NumericUpDown();
            this.lblPerCopyStartRcpt = new System.Windows.Forms.Label();
            this.numerPerCopyEndZ = new System.Windows.Forms.NumericUpDown();
            this.numerPerCopyStartZ = new System.Windows.Forms.NumericUpDown();
            this.lblPerCopyEndZ = new System.Windows.Forms.Label();
            this.lblPerCopyStartZ = new System.Windows.Forms.Label();
            this.btnPerZR = new Hugin.POS.MyButton();
            this.tmPerCopyEndTime = new Hugin.POS.TimePicker(this.components);
            this.tmPerCopyStartTime = new Hugin.POS.TimePicker(this.components);
            this.lblPerCopyEndTime = new System.Windows.Forms.Label();
            this.lblPerCopyStartTime = new System.Windows.Forms.Label();
            this.btnPerCopyDates = new Hugin.POS.MyButton();
            this.dtPerCopyEndDate = new System.Windows.Forms.DateTimePicker();
            this.dtPerCopyStartDate = new System.Windows.Forms.DateTimePicker();
            this.lblPerCopyEndDate = new System.Windows.Forms.Label();
            this.lblPerCopyStartDate = new System.Windows.Forms.Label();
            this.btnFMBwDates = new Hugin.POS.MyButton();
            this.dtFMEndDate = new System.Windows.Forms.DateTimePicker();
            this.dtFMStartDate = new System.Windows.Forms.DateTimePicker();
            this.lblFMEndDate = new System.Windows.Forms.Label();
            this.lblFMStartDate = new System.Windows.Forms.Label();
            this.numerFMEndZ = new System.Windows.Forms.NumericUpDown();
            this.numerFMStartZ = new System.Windows.Forms.NumericUpDown();
            this.lblFMEndZ = new System.Windows.Forms.Label();
            this.lblFMStartZ = new System.Windows.Forms.Label();
            this.btnFMBwZ = new Hugin.POS.MyButton();
            this.btnPrgReport = new Hugin.POS.MyButton();
            this.btnSaleReport = new Hugin.POS.MyButton();
            this.btnEJDetail = new Hugin.POS.MyButton();
            this.btnXReport = new Hugin.POS.MyButton();
            this.btnZReport = new Hugin.POS.MyButton();
            this.timerMain = new System.Windows.Forms.Timer(this.components);
            this.pnlMain = new System.Windows.Forms.Panel();
            this.btnReturnSale = new Hugin.POS.MyButton();
            this.btnEJPeriodic = new Hugin.POS.MyButton();
            this.btnEJSingle = new Hugin.POS.MyButton();
            this.btnFMDate = new Hugin.POS.MyButton();
            this.btnFMZZ = new Hugin.POS.MyButton();
            this.pnlFMZZ = new System.Windows.Forms.Panel();
            this.btnMainZZ = new Hugin.POS.MyButton();
            this.btnRetSaleZZ = new Hugin.POS.MyButton();
            this.pnlFMDate = new System.Windows.Forms.Panel();
            this.btnMainFMDate = new Hugin.POS.MyButton();
            this.btnRetSaleFMDate = new Hugin.POS.MyButton();
            this.pnlEJSingle = new System.Windows.Forms.Panel();
            this.btnEJSingDate = new Hugin.POS.MyButton();
            this.btnEJSingByZR = new Hugin.POS.MyButton();
            this.btnEJZCopy = new Hugin.POS.MyButton();
            this.btnMainEJSing = new Hugin.POS.MyButton();
            this.btnRetSaleEJSing = new Hugin.POS.MyButton();
            this.pnlEJPeriodic = new System.Windows.Forms.Panel();
            this.btnEJPerDaily = new Hugin.POS.MyButton();
            this.btnEJPerDate = new Hugin.POS.MyButton();
            this.btnEJPerZR = new Hugin.POS.MyButton();
            this.btnMainEJPer = new Hugin.POS.MyButton();
            this.btnRetSaleEJPer = new Hugin.POS.MyButton();
            this.pnlEJZCopy = new System.Windows.Forms.Panel();
            this.btnMainEJZCopy = new Hugin.POS.MyButton();
            this.btnRetSaleEJZCopy = new Hugin.POS.MyButton();
            this.pnlEJSingZR = new System.Windows.Forms.Panel();
            this.btnMainEJSingZR = new Hugin.POS.MyButton();
            this.btnRetSaleEJSingZR = new Hugin.POS.MyButton();
            this.pnlEJSingDate = new System.Windows.Forms.Panel();
            this.btnMainEJSingDate = new Hugin.POS.MyButton();
            this.btnRetSaleEJSingDate = new Hugin.POS.MyButton();
            this.pnlEJPerZR = new System.Windows.Forms.Panel();
            this.btnMainEJPerZR = new Hugin.POS.MyButton();
            this.btnRetSaleZR = new Hugin.POS.MyButton();
            this.pnlEJPerDate = new System.Windows.Forms.Panel();
            this.btnMainEJPerDate = new Hugin.POS.MyButton();
            this.btnRetSaleEJPerDate = new Hugin.POS.MyButton();
            this.pnlEJPerDaily.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numerZCopyZNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerSingleCopyRcptNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerSingleCopyZNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerPerCopyEndRcpt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerPerCopyStartRcpt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerPerCopyEndZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerPerCopyStartZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerFMEndZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerFMStartZ)).BeginInit();
            this.pnlMain.SuspendLayout();
            this.pnlFMZZ.SuspendLayout();
            this.pnlFMDate.SuspendLayout();
            this.pnlEJSingle.SuspendLayout();
            this.pnlEJPeriodic.SuspendLayout();
            this.pnlEJZCopy.SuspendLayout();
            this.pnlEJSingZR.SuspendLayout();
            this.pnlEJSingDate.SuspendLayout();
            this.pnlEJPerZR.SuspendLayout();
            this.pnlEJPerDate.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlEJPerDaily
            // 
            this.pnlEJPerDaily.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlEJPerDaily.BackgroundImage")));
            this.pnlEJPerDaily.Controls.Add(this.btnEJDaily);
            this.pnlEJPerDaily.Controls.Add(this.dtEJPerDailyDate);
            this.pnlEJPerDaily.Controls.Add(this.lblEJPerDailyDate);
            this.pnlEJPerDaily.Controls.Add(this.btnMainEJPerDaily);
            this.pnlEJPerDaily.Controls.Add(this.btnRetSaleEJPerDaily);
            this.pnlEJPerDaily.Font = new System.Drawing.Font("Lucida Console", 14F);
            this.pnlEJPerDaily.Location = new System.Drawing.Point(0, 0);
            this.pnlEJPerDaily.Name = "pnlEJPerDaily";
            this.pnlEJPerDaily.Size = new System.Drawing.Size(305, 270);
            this.pnlEJPerDaily.TabIndex = 3;
            // 
            // btnEJDaily
            // 
            this.btnEJDaily.BackColor = System.Drawing.Color.Transparent;
            this.btnEJDaily.DisableColor = System.Drawing.Color.LightGray;
            this.btnEJDaily.Location = new System.Drawing.Point(210, 210);
            this.btnEJDaily.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnEJDaily.MForeColor = System.Drawing.Color.Black;
            this.btnEJDaily.MHeight = 52;
            this.btnEJDaily.MText = "";
            this.btnEJDaily.MWidth = 85;
            this.btnEJDaily.Name = "btnEJDaily";
            this.btnEJDaily.Size = new System.Drawing.Size(85, 52);
            this.btnEJDaily.TabIndex = 15;
            this.btnEJDaily.Click += new System.EventHandler(this.btnEJDaily_Click);
            // 
            // dtEJPerDailyDate
            // 
            this.dtEJPerDailyDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtEJPerDailyDate.Location = new System.Drawing.Point(145, 34);
            this.dtEJPerDailyDate.Name = "dtEJPerDailyDate";
            this.dtEJPerDailyDate.Size = new System.Drawing.Size(139, 26);
            this.dtEJPerDailyDate.TabIndex = 14;
            // 
            // lblEJPerDailyDate
            // 
            this.lblEJPerDailyDate.AutoSize = true;
            this.lblEJPerDailyDate.Location = new System.Drawing.Point(6, 37);
            this.lblEJPerDailyDate.Name = "lblEJPerDailyDate";
            this.lblEJPerDailyDate.Size = new System.Drawing.Size(75, 19);
            this.lblEJPerDailyDate.TabIndex = 13;
            this.lblEJPerDailyDate.Text = "TARİH:";
            // 
            // btnMainEJPerDaily
            // 
            this.btnMainEJPerDaily.BackColor = System.Drawing.Color.Transparent;
            this.btnMainEJPerDaily.DisableColor = System.Drawing.Color.LightGray;
            this.btnMainEJPerDaily.Location = new System.Drawing.Point(10, 208);
            this.btnMainEJPerDaily.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnMainEJPerDaily.MForeColor = System.Drawing.Color.Black;
            this.btnMainEJPerDaily.MHeight = 52;
            this.btnMainEJPerDaily.MText = "";
            this.btnMainEJPerDaily.MWidth = 85;
            this.btnMainEJPerDaily.Name = "btnMainEJPerDaily";
            this.btnMainEJPerDaily.Size = new System.Drawing.Size(85, 52);
            this.btnMainEJPerDaily.TabIndex = 12;
            this.btnMainEJPerDaily.Click += new System.EventHandler(this.OnMainPage);
            // 
            // btnRetSaleEJPerDaily
            // 
            this.btnRetSaleEJPerDaily.BackColor = System.Drawing.Color.Transparent;
            this.btnRetSaleEJPerDaily.DisableColor = System.Drawing.Color.LightGray;
            this.btnRetSaleEJPerDaily.Location = new System.Drawing.Point(109, 209);
            this.btnRetSaleEJPerDaily.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnRetSaleEJPerDaily.MForeColor = System.Drawing.Color.Black;
            this.btnRetSaleEJPerDaily.MHeight = 52;
            this.btnRetSaleEJPerDaily.MText = "";
            this.btnRetSaleEJPerDaily.MWidth = 85;
            this.btnRetSaleEJPerDaily.Name = "btnRetSaleEJPerDaily";
            this.btnRetSaleEJPerDaily.Size = new System.Drawing.Size(85, 52);
            this.btnRetSaleEJPerDaily.TabIndex = 11;
            this.btnRetSaleEJPerDaily.Click += new System.EventHandler(this.OnReturnSale);
            // 
            // numerZCopyZNo
            // 
            this.numerZCopyZNo.Location = new System.Drawing.Point(158, 40);
            this.numerZCopyZNo.Maximum = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            this.numerZCopyZNo.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerZCopyZNo.Name = "numerZCopyZNo";
            this.numerZCopyZNo.Size = new System.Drawing.Size(120, 26);
            this.numerZCopyZNo.TabIndex = 8;
            this.numerZCopyZNo.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerZCopyZNo.Enter += new System.EventHandler(this.numeric_Enter);
            // 
            // lblZCopyZNo
            // 
            this.lblZCopyZNo.AutoSize = true;
            this.lblZCopyZNo.Location = new System.Drawing.Point(7, 43);
            this.lblZCopyZNo.Name = "lblZCopyZNo";
            this.lblZCopyZNo.Size = new System.Drawing.Size(130, 19);
            this.lblZCopyZNo.TabIndex = 6;
            this.lblZCopyZNo.Text = "Z NUMARASI:";
            // 
            // btnZCopy
            // 
            this.btnZCopy.BackColor = System.Drawing.Color.Transparent;
            this.btnZCopy.DisableColor = System.Drawing.Color.LightGray;
            this.btnZCopy.Location = new System.Drawing.Point(210, 209);
            this.btnZCopy.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnZCopy.MForeColor = System.Drawing.Color.Black;
            this.btnZCopy.MHeight = 52;
            this.btnZCopy.MText = "";
            this.btnZCopy.MWidth = 85;
            this.btnZCopy.Name = "btnZCopy";
            this.btnZCopy.Size = new System.Drawing.Size(85, 52);
            this.btnZCopy.TabIndex = 5;
            this.btnZCopy.Click += new System.EventHandler(this.btnZCopy_Click);
            // 
            // numerSingleCopyRcptNo
            // 
            this.numerSingleCopyRcptNo.Location = new System.Drawing.Point(172, 59);
            this.numerSingleCopyRcptNo.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numerSingleCopyRcptNo.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerSingleCopyRcptNo.Name = "numerSingleCopyRcptNo";
            this.numerSingleCopyRcptNo.Size = new System.Drawing.Size(120, 26);
            this.numerSingleCopyRcptNo.TabIndex = 13;
            this.numerSingleCopyRcptNo.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerSingleCopyRcptNo.Enter += new System.EventHandler(this.numeric_Enter);
            // 
            // numerSingleCopyZNo
            // 
            this.numerSingleCopyZNo.Location = new System.Drawing.Point(172, 27);
            this.numerSingleCopyZNo.Maximum = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            this.numerSingleCopyZNo.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerSingleCopyZNo.Name = "numerSingleCopyZNo";
            this.numerSingleCopyZNo.Size = new System.Drawing.Size(120, 26);
            this.numerSingleCopyZNo.TabIndex = 12;
            this.numerSingleCopyZNo.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerSingleCopyZNo.Enter += new System.EventHandler(this.numeric_Enter);
            // 
            // lblSingleCopyRcptNo
            // 
            this.lblSingleCopyRcptNo.AutoSize = true;
            this.lblSingleCopyRcptNo.Location = new System.Drawing.Point(7, 65);
            this.lblSingleCopyRcptNo.Name = "lblSingleCopyRcptNo";
            this.lblSingleCopyRcptNo.Size = new System.Drawing.Size(152, 19);
            this.lblSingleCopyRcptNo.TabIndex = 11;
            this.lblSingleCopyRcptNo.Text = "BELGE FİŞ NO:";
            // 
            // lblSingleCopyZNo
            // 
            this.lblSingleCopyZNo.AutoSize = true;
            this.lblSingleCopyZNo.Location = new System.Drawing.Point(7, 30);
            this.lblSingleCopyZNo.Name = "lblSingleCopyZNo";
            this.lblSingleCopyZNo.Size = new System.Drawing.Size(130, 19);
            this.lblSingleCopyZNo.TabIndex = 10;
            this.lblSingleCopyZNo.Text = "BELGE Z NO:";
            // 
            // btnDocCopy
            // 
            this.btnDocCopy.BackColor = System.Drawing.Color.Transparent;
            this.btnDocCopy.DisableColor = System.Drawing.Color.LightGray;
            this.btnDocCopy.Location = new System.Drawing.Point(210, 209);
            this.btnDocCopy.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnDocCopy.MForeColor = System.Drawing.Color.Black;
            this.btnDocCopy.MHeight = 52;
            this.btnDocCopy.MText = "";
            this.btnDocCopy.MWidth = 85;
            this.btnDocCopy.Name = "btnDocCopy";
            this.btnDocCopy.Size = new System.Drawing.Size(85, 52);
            this.btnDocCopy.TabIndex = 4;
            this.btnDocCopy.Click += new System.EventHandler(this.btnDocCopy_Click);
            // 
            // tmSingleCopyTime
            // 
            this.tmSingleCopyTime.Location = new System.Drawing.Point(147, 70);
            this.tmSingleCopyTime.Name = "tmSingleCopyTime";
            this.tmSingleCopyTime.Size = new System.Drawing.Size(100, 26);
            this.tmSingleCopyTime.TabIndex = 14;
            this.tmSingleCopyTime.Text = "16:19";
            this.tmSingleCopyTime.Value = new System.DateTime(2000, 1, 1, 16, 19, 0, 0);
            this.tmSingleCopyTime.Enter += new System.EventHandler(this.timePicker_Enter);
            // 
            // btnDateCopy
            // 
            this.btnDateCopy.BackColor = System.Drawing.Color.Transparent;
            this.btnDateCopy.DisableColor = System.Drawing.Color.LightGray;
            this.btnDateCopy.Location = new System.Drawing.Point(210, 209);
            this.btnDateCopy.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnDateCopy.MForeColor = System.Drawing.Color.Black;
            this.btnDateCopy.MHeight = 52;
            this.btnDateCopy.MText = "";
            this.btnDateCopy.MWidth = 85;
            this.btnDateCopy.Name = "btnDateCopy";
            this.btnDateCopy.Size = new System.Drawing.Size(85, 52);
            this.btnDateCopy.TabIndex = 9;
            this.btnDateCopy.Click += new System.EventHandler(this.btnDateCopy_Click);
            // 
            // dtSingleCopyDate
            // 
            this.dtSingleCopyDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtSingleCopyDate.Location = new System.Drawing.Point(147, 35);
            this.dtSingleCopyDate.Name = "dtSingleCopyDate";
            this.dtSingleCopyDate.Size = new System.Drawing.Size(149, 26);
            this.dtSingleCopyDate.TabIndex = 7;
            // 
            // lblSingleCopyTime
            // 
            this.lblSingleCopyTime.AutoSize = true;
            this.lblSingleCopyTime.Location = new System.Drawing.Point(5, 72);
            this.lblSingleCopyTime.Name = "lblSingleCopyTime";
            this.lblSingleCopyTime.Size = new System.Drawing.Size(64, 19);
            this.lblSingleCopyTime.TabIndex = 6;
            this.lblSingleCopyTime.Text = "SAAT:";
            // 
            // lblSingleCopyDate
            // 
            this.lblSingleCopyDate.AutoSize = true;
            this.lblSingleCopyDate.Location = new System.Drawing.Point(5, 37);
            this.lblSingleCopyDate.Name = "lblSingleCopyDate";
            this.lblSingleCopyDate.Size = new System.Drawing.Size(75, 19);
            this.lblSingleCopyDate.TabIndex = 5;
            this.lblSingleCopyDate.Text = "TARİH:";
            // 
            // numerPerCopyEndRcpt
            // 
            this.numerPerCopyEndRcpt.Location = new System.Drawing.Point(172, 133);
            this.numerPerCopyEndRcpt.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numerPerCopyEndRcpt.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerPerCopyEndRcpt.Name = "numerPerCopyEndRcpt";
            this.numerPerCopyEndRcpt.Size = new System.Drawing.Size(120, 26);
            this.numerPerCopyEndRcpt.TabIndex = 17;
            this.numerPerCopyEndRcpt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerPerCopyEndRcpt.Enter += new System.EventHandler(this.numeric_Enter);
            // 
            // lblPerCopyEndRcpt
            // 
            this.lblPerCopyEndRcpt.AutoSize = true;
            this.lblPerCopyEndRcpt.Location = new System.Drawing.Point(6, 137);
            this.lblPerCopyEndRcpt.Name = "lblPerCopyEndRcpt";
            this.lblPerCopyEndRcpt.Size = new System.Drawing.Size(130, 19);
            this.lblPerCopyEndRcpt.TabIndex = 16;
            this.lblPerCopyEndRcpt.Text = "SON FİŞ NO:";
            // 
            // numerPerCopyStartRcpt
            // 
            this.numerPerCopyStartRcpt.Location = new System.Drawing.Point(172, 63);
            this.numerPerCopyStartRcpt.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numerPerCopyStartRcpt.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerPerCopyStartRcpt.Name = "numerPerCopyStartRcpt";
            this.numerPerCopyStartRcpt.Size = new System.Drawing.Size(120, 26);
            this.numerPerCopyStartRcpt.TabIndex = 15;
            this.numerPerCopyStartRcpt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerPerCopyStartRcpt.Enter += new System.EventHandler(this.numeric_Enter);
            // 
            // lblPerCopyStartRcpt
            // 
            this.lblPerCopyStartRcpt.AutoSize = true;
            this.lblPerCopyStartRcpt.Location = new System.Drawing.Point(6, 67);
            this.lblPerCopyStartRcpt.Name = "lblPerCopyStartRcpt";
            this.lblPerCopyStartRcpt.Size = new System.Drawing.Size(130, 19);
            this.lblPerCopyStartRcpt.TabIndex = 14;
            this.lblPerCopyStartRcpt.Text = "İLK FİŞ NO:";
            // 
            // numerPerCopyEndZ
            // 
            this.numerPerCopyEndZ.Location = new System.Drawing.Point(172, 98);
            this.numerPerCopyEndZ.Maximum = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            this.numerPerCopyEndZ.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerPerCopyEndZ.Name = "numerPerCopyEndZ";
            this.numerPerCopyEndZ.Size = new System.Drawing.Size(120, 26);
            this.numerPerCopyEndZ.TabIndex = 9;
            this.numerPerCopyEndZ.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerPerCopyEndZ.ValueChanged += new System.EventHandler(this.numerPerCopyEndZ_ValueChanged);
            this.numerPerCopyEndZ.Enter += new System.EventHandler(this.numeric_Enter);
            // 
            // numerPerCopyStartZ
            // 
            this.numerPerCopyStartZ.Location = new System.Drawing.Point(172, 28);
            this.numerPerCopyStartZ.Maximum = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            this.numerPerCopyStartZ.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerPerCopyStartZ.Name = "numerPerCopyStartZ";
            this.numerPerCopyStartZ.Size = new System.Drawing.Size(120, 26);
            this.numerPerCopyStartZ.TabIndex = 8;
            this.numerPerCopyStartZ.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerPerCopyStartZ.ValueChanged += new System.EventHandler(this.numerPerCopyStartZ_ValueChanged);
            this.numerPerCopyStartZ.Enter += new System.EventHandler(this.numeric_Enter);
            // 
            // lblPerCopyEndZ
            // 
            this.lblPerCopyEndZ.AutoSize = true;
            this.lblPerCopyEndZ.Location = new System.Drawing.Point(6, 102);
            this.lblPerCopyEndZ.Name = "lblPerCopyEndZ";
            this.lblPerCopyEndZ.Size = new System.Drawing.Size(108, 19);
            this.lblPerCopyEndZ.TabIndex = 7;
            this.lblPerCopyEndZ.Text = "SON Z NO:";
            // 
            // lblPerCopyStartZ
            // 
            this.lblPerCopyStartZ.AutoSize = true;
            this.lblPerCopyStartZ.Location = new System.Drawing.Point(6, 32);
            this.lblPerCopyStartZ.Name = "lblPerCopyStartZ";
            this.lblPerCopyStartZ.Size = new System.Drawing.Size(108, 19);
            this.lblPerCopyStartZ.TabIndex = 6;
            this.lblPerCopyStartZ.Text = "İLK Z NO:";
            // 
            // btnPerZR
            // 
            this.btnPerZR.BackColor = System.Drawing.Color.Transparent;
            this.btnPerZR.DisableColor = System.Drawing.Color.LightGray;
            this.btnPerZR.Location = new System.Drawing.Point(210, 209);
            this.btnPerZR.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnPerZR.MForeColor = System.Drawing.Color.Black;
            this.btnPerZR.MHeight = 52;
            this.btnPerZR.MText = "";
            this.btnPerZR.MWidth = 85;
            this.btnPerZR.Name = "btnPerZR";
            this.btnPerZR.Size = new System.Drawing.Size(85, 52);
            this.btnPerZR.TabIndex = 5;
            this.btnPerZR.Click += new System.EventHandler(this.btnPerZR_Click);
            // 
            // tmPerCopyEndTime
            // 
            this.tmPerCopyEndTime.Location = new System.Drawing.Point(143, 134);
            this.tmPerCopyEndTime.Name = "tmPerCopyEndTime";
            this.tmPerCopyEndTime.Size = new System.Drawing.Size(100, 26);
            this.tmPerCopyEndTime.TabIndex = 14;
            this.tmPerCopyEndTime.Text = "16:19";
            this.tmPerCopyEndTime.Value = new System.DateTime(2000, 1, 1, 16, 19, 0, 0);
            this.tmPerCopyEndTime.Enter += new System.EventHandler(this.timePicker_Enter);
            // 
            // tmPerCopyStartTime
            // 
            this.tmPerCopyStartTime.Location = new System.Drawing.Point(143, 65);
            this.tmPerCopyStartTime.Name = "tmPerCopyStartTime";
            this.tmPerCopyStartTime.Size = new System.Drawing.Size(100, 26);
            this.tmPerCopyStartTime.TabIndex = 13;
            this.tmPerCopyStartTime.Text = "16:19";
            this.tmPerCopyStartTime.Value = new System.DateTime(2000, 1, 1, 16, 19, 0, 0);
            this.tmPerCopyStartTime.Enter += new System.EventHandler(this.timePicker_Enter);
            // 
            // lblPerCopyEndTime
            // 
            this.lblPerCopyEndTime.AutoSize = true;
            this.lblPerCopyEndTime.Location = new System.Drawing.Point(4, 137);
            this.lblPerCopyEndTime.Name = "lblPerCopyEndTime";
            this.lblPerCopyEndTime.Size = new System.Drawing.Size(108, 19);
            this.lblPerCopyEndTime.TabIndex = 11;
            this.lblPerCopyEndTime.Text = "SON SAAT:";
            // 
            // lblPerCopyStartTime
            // 
            this.lblPerCopyStartTime.AutoSize = true;
            this.lblPerCopyStartTime.Location = new System.Drawing.Point(4, 67);
            this.lblPerCopyStartTime.Name = "lblPerCopyStartTime";
            this.lblPerCopyStartTime.Size = new System.Drawing.Size(108, 19);
            this.lblPerCopyStartTime.TabIndex = 9;
            this.lblPerCopyStartTime.Text = "İLK SAAT:";
            // 
            // btnPerCopyDates
            // 
            this.btnPerCopyDates.BackColor = System.Drawing.Color.Transparent;
            this.btnPerCopyDates.DisableColor = System.Drawing.Color.LightGray;
            this.btnPerCopyDates.Location = new System.Drawing.Point(211, 209);
            this.btnPerCopyDates.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnPerCopyDates.MForeColor = System.Drawing.Color.Black;
            this.btnPerCopyDates.MHeight = 52;
            this.btnPerCopyDates.MText = "";
            this.btnPerCopyDates.MWidth = 85;
            this.btnPerCopyDates.Name = "btnPerCopyDates";
            this.btnPerCopyDates.Size = new System.Drawing.Size(85, 52);
            this.btnPerCopyDates.TabIndex = 4;
            this.btnPerCopyDates.Click += new System.EventHandler(this.btnPerCopyDates_Click);
            // 
            // dtPerCopyEndDate
            // 
            this.dtPerCopyEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtPerCopyEndDate.Location = new System.Drawing.Point(143, 99);
            this.dtPerCopyEndDate.Name = "dtPerCopyEndDate";
            this.dtPerCopyEndDate.Size = new System.Drawing.Size(139, 26);
            this.dtPerCopyEndDate.TabIndex = 3;
            this.dtPerCopyEndDate.ValueChanged += new System.EventHandler(this.dtPerCopyEndDate_ValueChanged);
            // 
            // dtPerCopyStartDate
            // 
            this.dtPerCopyStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtPerCopyStartDate.Location = new System.Drawing.Point(143, 29);
            this.dtPerCopyStartDate.Name = "dtPerCopyStartDate";
            this.dtPerCopyStartDate.Size = new System.Drawing.Size(139, 26);
            this.dtPerCopyStartDate.TabIndex = 2;
            this.dtPerCopyStartDate.ValueChanged += new System.EventHandler(this.dtPerCopyStartDate_ValueChanged);
            // 
            // lblPerCopyEndDate
            // 
            this.lblPerCopyEndDate.AutoSize = true;
            this.lblPerCopyEndDate.Location = new System.Drawing.Point(4, 102);
            this.lblPerCopyEndDate.Name = "lblPerCopyEndDate";
            this.lblPerCopyEndDate.Size = new System.Drawing.Size(119, 19);
            this.lblPerCopyEndDate.TabIndex = 1;
            this.lblPerCopyEndDate.Text = "SON TARİH:";
            // 
            // lblPerCopyStartDate
            // 
            this.lblPerCopyStartDate.AutoSize = true;
            this.lblPerCopyStartDate.Location = new System.Drawing.Point(4, 32);
            this.lblPerCopyStartDate.Name = "lblPerCopyStartDate";
            this.lblPerCopyStartDate.Size = new System.Drawing.Size(119, 19);
            this.lblPerCopyStartDate.TabIndex = 0;
            this.lblPerCopyStartDate.Text = "İLK TARİH:";
            // 
            // btnFMBwDates
            // 
            this.btnFMBwDates.BackColor = System.Drawing.Color.Transparent;
            this.btnFMBwDates.DisableColor = System.Drawing.Color.LightGray;
            this.btnFMBwDates.Location = new System.Drawing.Point(210, 209);
            this.btnFMBwDates.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnFMBwDates.MForeColor = System.Drawing.Color.Black;
            this.btnFMBwDates.MHeight = 52;
            this.btnFMBwDates.MText = "";
            this.btnFMBwDates.MWidth = 85;
            this.btnFMBwDates.Name = "btnFMBwDates";
            this.btnFMBwDates.Size = new System.Drawing.Size(85, 52);
            this.btnFMBwDates.TabIndex = 4;
            this.btnFMBwDates.Click += new System.EventHandler(this.btnFMBwDates_Click);
            // 
            // dtFMEndDate
            // 
            this.dtFMEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtFMEndDate.Location = new System.Drawing.Point(132, 60);
            this.dtFMEndDate.Name = "dtFMEndDate";
            this.dtFMEndDate.Size = new System.Drawing.Size(168, 26);
            this.dtFMEndDate.TabIndex = 3;
            this.dtFMEndDate.ValueChanged += new System.EventHandler(this.dtFMEndDate_ValueChanged);
            // 
            // dtFMStartDate
            // 
            this.dtFMStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtFMStartDate.Location = new System.Drawing.Point(132, 25);
            this.dtFMStartDate.Name = "dtFMStartDate";
            this.dtFMStartDate.Size = new System.Drawing.Size(170, 26);
            this.dtFMStartDate.TabIndex = 2;
            this.dtFMStartDate.ValueChanged += new System.EventHandler(this.dtFMStartDate_ValueChanged);
            // 
            // lblFMEndDate
            // 
            this.lblFMEndDate.AutoSize = true;
            this.lblFMEndDate.Location = new System.Drawing.Point(7, 63);
            this.lblFMEndDate.Name = "lblFMEndDate";
            this.lblFMEndDate.Size = new System.Drawing.Size(119, 19);
            this.lblFMEndDate.TabIndex = 1;
            this.lblFMEndDate.Text = "SON TARİH:";
            // 
            // lblFMStartDate
            // 
            this.lblFMStartDate.AutoSize = true;
            this.lblFMStartDate.Location = new System.Drawing.Point(7, 28);
            this.lblFMStartDate.Name = "lblFMStartDate";
            this.lblFMStartDate.Size = new System.Drawing.Size(119, 19);
            this.lblFMStartDate.TabIndex = 0;
            this.lblFMStartDate.Text = "İLK TARİH:";
            // 
            // numerFMEndZ
            // 
            this.numerFMEndZ.Location = new System.Drawing.Point(180, 67);
            this.numerFMEndZ.Maximum = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            this.numerFMEndZ.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerFMEndZ.Name = "numerFMEndZ";
            this.numerFMEndZ.Size = new System.Drawing.Size(120, 26);
            this.numerFMEndZ.TabIndex = 9;
            this.numerFMEndZ.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerFMEndZ.ValueChanged += new System.EventHandler(this.numerFMEndZ_ValueChanged);
            this.numerFMEndZ.Enter += new System.EventHandler(this.numeric_Enter);
            // 
            // numerFMStartZ
            // 
            this.numerFMStartZ.Location = new System.Drawing.Point(180, 32);
            this.numerFMStartZ.Maximum = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            this.numerFMStartZ.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerFMStartZ.Name = "numerFMStartZ";
            this.numerFMStartZ.Size = new System.Drawing.Size(120, 26);
            this.numerFMStartZ.TabIndex = 8;
            this.numerFMStartZ.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerFMStartZ.ValueChanged += new System.EventHandler(this.numerFMStartZ_ValueChanged);
            this.numerFMStartZ.Enter += new System.EventHandler(this.numeric_Enter);
            // 
            // lblFMEndZ
            // 
            this.lblFMEndZ.AutoSize = true;
            this.lblFMEndZ.Location = new System.Drawing.Point(3, 68);
            this.lblFMEndZ.Name = "lblFMEndZ";
            this.lblFMEndZ.Size = new System.Drawing.Size(108, 19);
            this.lblFMEndZ.TabIndex = 7;
            this.lblFMEndZ.Text = "SON Z NO:";
            // 
            // lblFMStartZ
            // 
            this.lblFMStartZ.AutoSize = true;
            this.lblFMStartZ.Location = new System.Drawing.Point(3, 33);
            this.lblFMStartZ.Name = "lblFMStartZ";
            this.lblFMStartZ.Size = new System.Drawing.Size(108, 19);
            this.lblFMStartZ.TabIndex = 6;
            this.lblFMStartZ.Text = "İLK Z NO:";
            // 
            // btnFMBwZ
            // 
            this.btnFMBwZ.BackColor = System.Drawing.Color.Transparent;
            this.btnFMBwZ.DisableColor = System.Drawing.Color.LightGray;
            this.btnFMBwZ.Location = new System.Drawing.Point(210, 209);
            this.btnFMBwZ.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnFMBwZ.MForeColor = System.Drawing.Color.Black;
            this.btnFMBwZ.MHeight = 52;
            this.btnFMBwZ.MText = "";
            this.btnFMBwZ.MWidth = 85;
            this.btnFMBwZ.Name = "btnFMBwZ";
            this.btnFMBwZ.Size = new System.Drawing.Size(85, 52);
            this.btnFMBwZ.TabIndex = 5;
            this.btnFMBwZ.Click += new System.EventHandler(this.btnFMBwZ_Click);
            // 
            // btnPrgReport
            // 
            this.btnPrgReport.BackColor = System.Drawing.Color.Transparent;
            this.btnPrgReport.DisableColor = System.Drawing.Color.LightGray;
            this.btnPrgReport.Location = new System.Drawing.Point(9, 139);
            this.btnPrgReport.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnPrgReport.MForeColor = System.Drawing.Color.Black;
            this.btnPrgReport.MHeight = 52;
            this.btnPrgReport.MText = "";
            this.btnPrgReport.MWidth = 85;
            this.btnPrgReport.Name = "btnPrgReport";
            this.btnPrgReport.Size = new System.Drawing.Size(85, 52);
            this.btnPrgReport.TabIndex = 4;
            this.btnPrgReport.Click += new System.EventHandler(this.btnPrgReport_Click);
            // 
            // btnSaleReport
            // 
            this.btnSaleReport.BackColor = System.Drawing.Color.Transparent;
            this.btnSaleReport.DisableColor = System.Drawing.Color.LightGray;
            this.btnSaleReport.Location = new System.Drawing.Point(7, 70);
            this.btnSaleReport.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnSaleReport.MForeColor = System.Drawing.Color.Black;
            this.btnSaleReport.MHeight = 52;
            this.btnSaleReport.MText = "";
            this.btnSaleReport.MWidth = 85;
            this.btnSaleReport.Name = "btnSaleReport";
            this.btnSaleReport.Size = new System.Drawing.Size(85, 52);
            this.btnSaleReport.TabIndex = 3;
            this.btnSaleReport.Click += new System.EventHandler(this.btnSaleReport_Click);
            // 
            // btnEJDetail
            // 
            this.btnEJDetail.BackColor = System.Drawing.Color.Transparent;
            this.btnEJDetail.DisableColor = System.Drawing.Color.LightGray;
            this.btnEJDetail.Location = new System.Drawing.Point(211, 6);
            this.btnEJDetail.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnEJDetail.MForeColor = System.Drawing.Color.Black;
            this.btnEJDetail.MHeight = 52;
            this.btnEJDetail.MText = "";
            this.btnEJDetail.MWidth = 85;
            this.btnEJDetail.Name = "btnEJDetail";
            this.btnEJDetail.Size = new System.Drawing.Size(85, 52);
            this.btnEJDetail.TabIndex = 2;
            this.btnEJDetail.Click += new System.EventHandler(this.btnEJDetail_Click);
            // 
            // btnXReport
            // 
            this.btnXReport.BackColor = System.Drawing.Color.Transparent;
            this.btnXReport.DisableColor = System.Drawing.Color.LightGray;
            this.btnXReport.Location = new System.Drawing.Point(7, 5);
            this.btnXReport.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnXReport.MForeColor = System.Drawing.Color.Black;
            this.btnXReport.MHeight = 52;
            this.btnXReport.MText = "";
            this.btnXReport.MWidth = 85;
            this.btnXReport.Name = "btnXReport";
            this.btnXReport.Size = new System.Drawing.Size(85, 52);
            this.btnXReport.TabIndex = 1;
            this.btnXReport.Click += new System.EventHandler(this.btnXReport_Click);
            // 
            // btnZReport
            // 
            this.btnZReport.BackColor = System.Drawing.Color.Transparent;
            this.btnZReport.DisableColor = System.Drawing.Color.LightGray;
            this.btnZReport.Location = new System.Drawing.Point(109, 5);
            this.btnZReport.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnZReport.MForeColor = System.Drawing.Color.Black;
            this.btnZReport.MHeight = 52;
            this.btnZReport.MText = "";
            this.btnZReport.MWidth = 85;
            this.btnZReport.Name = "btnZReport";
            this.btnZReport.Size = new System.Drawing.Size(85, 52);
            this.btnZReport.TabIndex = 0;
            this.btnZReport.Click += new System.EventHandler(this.btnZReport_Click);
            // 
            // timerMain
            // 
            this.timerMain.Tick += new System.EventHandler(this.timerMain_Tick);
            // 
            // pnlMain
            // 
            this.pnlMain.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlMain.BackgroundImage")));
            this.pnlMain.Controls.Add(this.btnReturnSale);
            this.pnlMain.Controls.Add(this.btnEJPeriodic);
            this.pnlMain.Controls.Add(this.btnEJSingle);
            this.pnlMain.Controls.Add(this.btnFMDate);
            this.pnlMain.Controls.Add(this.btnFMZZ);
            this.pnlMain.Controls.Add(this.btnPrgReport);
            this.pnlMain.Controls.Add(this.btnZReport);
            this.pnlMain.Controls.Add(this.btnSaleReport);
            this.pnlMain.Controls.Add(this.btnXReport);
            this.pnlMain.Controls.Add(this.btnEJDetail);
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(305, 270);
            this.pnlMain.TabIndex = 1;
            // 
            // btnReturnSale
            // 
            this.btnReturnSale.BackColor = System.Drawing.Color.Transparent;
            this.btnReturnSale.DisableColor = System.Drawing.Color.LightGray;
            this.btnReturnSale.Location = new System.Drawing.Point(109, 209);
            this.btnReturnSale.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnReturnSale.MForeColor = System.Drawing.Color.Black;
            this.btnReturnSale.MHeight = 52;
            this.btnReturnSale.MText = "";
            this.btnReturnSale.MWidth = 85;
            this.btnReturnSale.Name = "btnReturnSale";
            this.btnReturnSale.Size = new System.Drawing.Size(85, 52);
            this.btnReturnSale.TabIndex = 9;
            this.btnReturnSale.Click += new System.EventHandler(this.OnReturnSale);
            // 
            // btnEJPeriodic
            // 
            this.btnEJPeriodic.BackColor = System.Drawing.Color.Transparent;
            this.btnEJPeriodic.DisableColor = System.Drawing.Color.LightGray;
            this.btnEJPeriodic.Location = new System.Drawing.Point(210, 134);
            this.btnEJPeriodic.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnEJPeriodic.MForeColor = System.Drawing.Color.Black;
            this.btnEJPeriodic.MHeight = 52;
            this.btnEJPeriodic.MText = "";
            this.btnEJPeriodic.MWidth = 85;
            this.btnEJPeriodic.Name = "btnEJPeriodic";
            this.btnEJPeriodic.Size = new System.Drawing.Size(85, 52);
            this.btnEJPeriodic.TabIndex = 8;
            this.btnEJPeriodic.Click += new System.EventHandler(this.btnEJPeriodic_Click);
            // 
            // btnEJSingle
            // 
            this.btnEJSingle.BackColor = System.Drawing.Color.Transparent;
            this.btnEJSingle.DisableColor = System.Drawing.Color.LightGray;
            this.btnEJSingle.Location = new System.Drawing.Point(211, 70);
            this.btnEJSingle.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnEJSingle.MForeColor = System.Drawing.Color.Black;
            this.btnEJSingle.MHeight = 52;
            this.btnEJSingle.MText = "";
            this.btnEJSingle.MWidth = 85;
            this.btnEJSingle.Name = "btnEJSingle";
            this.btnEJSingle.Size = new System.Drawing.Size(85, 52);
            this.btnEJSingle.TabIndex = 7;
            this.btnEJSingle.Click += new System.EventHandler(this.btnEJSingle_Click);
            // 
            // btnFMDate
            // 
            this.btnFMDate.BackColor = System.Drawing.Color.Transparent;
            this.btnFMDate.DisableColor = System.Drawing.Color.LightGray;
            this.btnFMDate.Location = new System.Drawing.Point(109, 138);
            this.btnFMDate.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnFMDate.MForeColor = System.Drawing.Color.Black;
            this.btnFMDate.MHeight = 52;
            this.btnFMDate.MText = "";
            this.btnFMDate.MWidth = 85;
            this.btnFMDate.Name = "btnFMDate";
            this.btnFMDate.Size = new System.Drawing.Size(85, 52);
            this.btnFMDate.TabIndex = 6;
            this.btnFMDate.Click += new System.EventHandler(this.btnFMDate_Click);
            // 
            // btnFMZZ
            // 
            this.btnFMZZ.BackColor = System.Drawing.Color.Transparent;
            this.btnFMZZ.DisableColor = System.Drawing.Color.LightGray;
            this.btnFMZZ.Location = new System.Drawing.Point(108, 70);
            this.btnFMZZ.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnFMZZ.MForeColor = System.Drawing.Color.Black;
            this.btnFMZZ.MHeight = 52;
            this.btnFMZZ.MText = "";
            this.btnFMZZ.MWidth = 85;
            this.btnFMZZ.Name = "btnFMZZ";
            this.btnFMZZ.Size = new System.Drawing.Size(85, 52);
            this.btnFMZZ.TabIndex = 5;
            this.btnFMZZ.Click += new System.EventHandler(this.btnFMZZ_Click);
            // 
            // pnlFMZZ
            // 
            this.pnlFMZZ.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlFMZZ.BackgroundImage")));
            this.pnlFMZZ.Controls.Add(this.numerFMEndZ);
            this.pnlFMZZ.Controls.Add(this.btnMainZZ);
            this.pnlFMZZ.Controls.Add(this.numerFMStartZ);
            this.pnlFMZZ.Controls.Add(this.btnRetSaleZZ);
            this.pnlFMZZ.Controls.Add(this.lblFMEndZ);
            this.pnlFMZZ.Controls.Add(this.btnFMBwZ);
            this.pnlFMZZ.Controls.Add(this.lblFMStartZ);
            this.pnlFMZZ.Font = new System.Drawing.Font("Lucida Console", 14F);
            this.pnlFMZZ.Location = new System.Drawing.Point(0, 0);
            this.pnlFMZZ.Name = "pnlFMZZ";
            this.pnlFMZZ.Size = new System.Drawing.Size(305, 270);
            this.pnlFMZZ.TabIndex = 0;
            // 
            // btnMainZZ
            // 
            this.btnMainZZ.BackColor = System.Drawing.Color.Transparent;
            this.btnMainZZ.DisableColor = System.Drawing.Color.LightGray;
            this.btnMainZZ.Location = new System.Drawing.Point(10, 207);
            this.btnMainZZ.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnMainZZ.MForeColor = System.Drawing.Color.Black;
            this.btnMainZZ.MHeight = 52;
            this.btnMainZZ.MText = "";
            this.btnMainZZ.MWidth = 85;
            this.btnMainZZ.Name = "btnMainZZ";
            this.btnMainZZ.Size = new System.Drawing.Size(85, 52);
            this.btnMainZZ.TabIndex = 6;
            this.btnMainZZ.Click += new System.EventHandler(this.OnMainPage);
            // 
            // btnRetSaleZZ
            // 
            this.btnRetSaleZZ.BackColor = System.Drawing.Color.Transparent;
            this.btnRetSaleZZ.DisableColor = System.Drawing.Color.LightGray;
            this.btnRetSaleZZ.Location = new System.Drawing.Point(109, 208);
            this.btnRetSaleZZ.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnRetSaleZZ.MForeColor = System.Drawing.Color.Black;
            this.btnRetSaleZZ.MHeight = 52;
            this.btnRetSaleZZ.MText = "";
            this.btnRetSaleZZ.MWidth = 85;
            this.btnRetSaleZZ.Name = "btnRetSaleZZ";
            this.btnRetSaleZZ.Size = new System.Drawing.Size(85, 52);
            this.btnRetSaleZZ.TabIndex = 6;
            this.btnRetSaleZZ.Click += new System.EventHandler(this.OnReturnSale);
            // 
            // pnlFMDate
            // 
            this.pnlFMDate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlFMDate.BackgroundImage")));
            this.pnlFMDate.Controls.Add(this.dtFMEndDate);
            this.pnlFMDate.Controls.Add(this.btnFMBwDates);
            this.pnlFMDate.Controls.Add(this.dtFMStartDate);
            this.pnlFMDate.Controls.Add(this.btnMainFMDate);
            this.pnlFMDate.Controls.Add(this.lblFMEndDate);
            this.pnlFMDate.Controls.Add(this.btnRetSaleFMDate);
            this.pnlFMDate.Controls.Add(this.lblFMStartDate);
            this.pnlFMDate.Font = new System.Drawing.Font("Lucida Console", 14F);
            this.pnlFMDate.Location = new System.Drawing.Point(0, 0);
            this.pnlFMDate.Name = "pnlFMDate";
            this.pnlFMDate.Size = new System.Drawing.Size(305, 270);
            this.pnlFMDate.TabIndex = 0;
            // 
            // btnMainFMDate
            // 
            this.btnMainFMDate.BackColor = System.Drawing.Color.Transparent;
            this.btnMainFMDate.DisableColor = System.Drawing.Color.LightGray;
            this.btnMainFMDate.Location = new System.Drawing.Point(11, 208);
            this.btnMainFMDate.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnMainFMDate.MForeColor = System.Drawing.Color.Black;
            this.btnMainFMDate.MHeight = 52;
            this.btnMainFMDate.MText = "";
            this.btnMainFMDate.MWidth = 85;
            this.btnMainFMDate.Name = "btnMainFMDate";
            this.btnMainFMDate.Size = new System.Drawing.Size(85, 52);
            this.btnMainFMDate.TabIndex = 8;
            this.btnMainFMDate.Click += new System.EventHandler(this.OnMainPage);
            // 
            // btnRetSaleFMDate
            // 
            this.btnRetSaleFMDate.BackColor = System.Drawing.Color.Transparent;
            this.btnRetSaleFMDate.DisableColor = System.Drawing.Color.LightGray;
            this.btnRetSaleFMDate.Location = new System.Drawing.Point(110, 209);
            this.btnRetSaleFMDate.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnRetSaleFMDate.MForeColor = System.Drawing.Color.Black;
            this.btnRetSaleFMDate.MHeight = 52;
            this.btnRetSaleFMDate.MText = "";
            this.btnRetSaleFMDate.MWidth = 85;
            this.btnRetSaleFMDate.Name = "btnRetSaleFMDate";
            this.btnRetSaleFMDate.Size = new System.Drawing.Size(85, 52);
            this.btnRetSaleFMDate.TabIndex = 7;
            this.btnRetSaleFMDate.Click += new System.EventHandler(this.OnReturnSale);
            // 
            // pnlEJSingle
            // 
            this.pnlEJSingle.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlEJSingle.BackgroundImage")));
            this.pnlEJSingle.Controls.Add(this.btnEJSingDate);
            this.pnlEJSingle.Controls.Add(this.btnEJSingByZR);
            this.pnlEJSingle.Controls.Add(this.btnEJZCopy);
            this.pnlEJSingle.Controls.Add(this.btnMainEJSing);
            this.pnlEJSingle.Controls.Add(this.btnRetSaleEJSing);
            this.pnlEJSingle.Location = new System.Drawing.Point(0, 0);
            this.pnlEJSingle.Name = "pnlEJSingle";
            this.pnlEJSingle.Size = new System.Drawing.Size(305, 270);
            this.pnlEJSingle.TabIndex = 0;
            // 
            // btnEJSingDate
            // 
            this.btnEJSingDate.BackColor = System.Drawing.Color.Transparent;
            this.btnEJSingDate.DisableColor = System.Drawing.Color.LightGray;
            this.btnEJSingDate.Location = new System.Drawing.Point(207, 52);
            this.btnEJSingDate.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnEJSingDate.MForeColor = System.Drawing.Color.Black;
            this.btnEJSingDate.MHeight = 52;
            this.btnEJSingDate.MText = "";
            this.btnEJSingDate.MWidth = 85;
            this.btnEJSingDate.Name = "btnEJSingDate";
            this.btnEJSingDate.Size = new System.Drawing.Size(85, 52);
            this.btnEJSingDate.TabIndex = 13;
            this.btnEJSingDate.Click += new System.EventHandler(this.btnEJSingDate_Click);
            // 
            // btnEJSingByZR
            // 
            this.btnEJSingByZR.BackColor = System.Drawing.Color.Transparent;
            this.btnEJSingByZR.DisableColor = System.Drawing.Color.LightGray;
            this.btnEJSingByZR.Location = new System.Drawing.Point(107, 52);
            this.btnEJSingByZR.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnEJSingByZR.MForeColor = System.Drawing.Color.Black;
            this.btnEJSingByZR.MHeight = 52;
            this.btnEJSingByZR.MText = "";
            this.btnEJSingByZR.MWidth = 85;
            this.btnEJSingByZR.Name = "btnEJSingByZR";
            this.btnEJSingByZR.Size = new System.Drawing.Size(85, 52);
            this.btnEJSingByZR.TabIndex = 12;
            this.btnEJSingByZR.Click += new System.EventHandler(this.btnEJSingByZR_Click);
            // 
            // btnEJZCopy
            // 
            this.btnEJZCopy.BackColor = System.Drawing.Color.Transparent;
            this.btnEJZCopy.DisableColor = System.Drawing.Color.LightGray;
            this.btnEJZCopy.Location = new System.Drawing.Point(8, 51);
            this.btnEJZCopy.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnEJZCopy.MForeColor = System.Drawing.Color.Black;
            this.btnEJZCopy.MHeight = 52;
            this.btnEJZCopy.MText = "";
            this.btnEJZCopy.MWidth = 85;
            this.btnEJZCopy.Name = "btnEJZCopy";
            this.btnEJZCopy.Size = new System.Drawing.Size(85, 52);
            this.btnEJZCopy.TabIndex = 11;
            this.btnEJZCopy.Click += new System.EventHandler(this.btnEJZCopy_Click);
            // 
            // btnMainEJSing
            // 
            this.btnMainEJSing.BackColor = System.Drawing.Color.Transparent;
            this.btnMainEJSing.DisableColor = System.Drawing.Color.LightGray;
            this.btnMainEJSing.Location = new System.Drawing.Point(10, 207);
            this.btnMainEJSing.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnMainEJSing.MForeColor = System.Drawing.Color.Black;
            this.btnMainEJSing.MHeight = 52;
            this.btnMainEJSing.MText = "";
            this.btnMainEJSing.MWidth = 85;
            this.btnMainEJSing.Name = "btnMainEJSing";
            this.btnMainEJSing.Size = new System.Drawing.Size(85, 52);
            this.btnMainEJSing.TabIndex = 10;
            this.btnMainEJSing.Click += new System.EventHandler(this.OnMainPage);
            // 
            // btnRetSaleEJSing
            // 
            this.btnRetSaleEJSing.BackColor = System.Drawing.Color.Transparent;
            this.btnRetSaleEJSing.DisableColor = System.Drawing.Color.LightGray;
            this.btnRetSaleEJSing.Location = new System.Drawing.Point(109, 208);
            this.btnRetSaleEJSing.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnRetSaleEJSing.MForeColor = System.Drawing.Color.Black;
            this.btnRetSaleEJSing.MHeight = 52;
            this.btnRetSaleEJSing.MText = "";
            this.btnRetSaleEJSing.MWidth = 85;
            this.btnRetSaleEJSing.Name = "btnRetSaleEJSing";
            this.btnRetSaleEJSing.Size = new System.Drawing.Size(85, 52);
            this.btnRetSaleEJSing.TabIndex = 9;
            this.btnRetSaleEJSing.Click += new System.EventHandler(this.OnReturnSale);
            // 
            // pnlEJPeriodic
            // 
            this.pnlEJPeriodic.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlEJPeriodic.BackgroundImage")));
            this.pnlEJPeriodic.Controls.Add(this.btnEJPerDaily);
            this.pnlEJPeriodic.Controls.Add(this.btnEJPerDate);
            this.pnlEJPeriodic.Controls.Add(this.btnEJPerZR);
            this.pnlEJPeriodic.Controls.Add(this.btnMainEJPer);
            this.pnlEJPeriodic.Controls.Add(this.btnRetSaleEJPer);
            this.pnlEJPeriodic.Location = new System.Drawing.Point(0, 0);
            this.pnlEJPeriodic.Name = "pnlEJPeriodic";
            this.pnlEJPeriodic.Size = new System.Drawing.Size(305, 270);
            this.pnlEJPeriodic.TabIndex = 1;
            // 
            // btnEJPerDaily
            // 
            this.btnEJPerDaily.BackColor = System.Drawing.Color.Transparent;
            this.btnEJPerDaily.DisableColor = System.Drawing.Color.LightGray;
            this.btnEJPerDaily.Location = new System.Drawing.Point(207, 52);
            this.btnEJPerDaily.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnEJPerDaily.MForeColor = System.Drawing.Color.Black;
            this.btnEJPerDaily.MHeight = 52;
            this.btnEJPerDaily.MText = "";
            this.btnEJPerDaily.MWidth = 85;
            this.btnEJPerDaily.Name = "btnEJPerDaily";
            this.btnEJPerDaily.Size = new System.Drawing.Size(85, 52);
            this.btnEJPerDaily.TabIndex = 13;
            this.btnEJPerDaily.Click += new System.EventHandler(this.btnEJPerDaily_Click);
            // 
            // btnEJPerDate
            // 
            this.btnEJPerDate.BackColor = System.Drawing.Color.Transparent;
            this.btnEJPerDate.DisableColor = System.Drawing.Color.LightGray;
            this.btnEJPerDate.Location = new System.Drawing.Point(107, 52);
            this.btnEJPerDate.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnEJPerDate.MForeColor = System.Drawing.Color.Black;
            this.btnEJPerDate.MHeight = 52;
            this.btnEJPerDate.MText = "";
            this.btnEJPerDate.MWidth = 85;
            this.btnEJPerDate.Name = "btnEJPerDate";
            this.btnEJPerDate.Size = new System.Drawing.Size(85, 52);
            this.btnEJPerDate.TabIndex = 12;
            this.btnEJPerDate.Click += new System.EventHandler(this.btnEJPerDate_Click);
            // 
            // btnEJPerZR
            // 
            this.btnEJPerZR.BackColor = System.Drawing.Color.Transparent;
            this.btnEJPerZR.DisableColor = System.Drawing.Color.LightGray;
            this.btnEJPerZR.Location = new System.Drawing.Point(8, 51);
            this.btnEJPerZR.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnEJPerZR.MForeColor = System.Drawing.Color.Black;
            this.btnEJPerZR.MHeight = 52;
            this.btnEJPerZR.MText = "";
            this.btnEJPerZR.MWidth = 85;
            this.btnEJPerZR.Name = "btnEJPerZR";
            this.btnEJPerZR.Size = new System.Drawing.Size(85, 52);
            this.btnEJPerZR.TabIndex = 11;
            this.btnEJPerZR.Click += new System.EventHandler(this.btnEJPerZR_Click);
            // 
            // btnMainEJPer
            // 
            this.btnMainEJPer.BackColor = System.Drawing.Color.Transparent;
            this.btnMainEJPer.DisableColor = System.Drawing.Color.LightGray;
            this.btnMainEJPer.Location = new System.Drawing.Point(10, 207);
            this.btnMainEJPer.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnMainEJPer.MForeColor = System.Drawing.Color.Black;
            this.btnMainEJPer.MHeight = 52;
            this.btnMainEJPer.MText = "";
            this.btnMainEJPer.MWidth = 85;
            this.btnMainEJPer.Name = "btnMainEJPer";
            this.btnMainEJPer.Size = new System.Drawing.Size(85, 52);
            this.btnMainEJPer.TabIndex = 10;
            this.btnMainEJPer.Click += new System.EventHandler(this.OnMainPage);
            // 
            // btnRetSaleEJPer
            // 
            this.btnRetSaleEJPer.BackColor = System.Drawing.Color.Transparent;
            this.btnRetSaleEJPer.DisableColor = System.Drawing.Color.LightGray;
            this.btnRetSaleEJPer.Location = new System.Drawing.Point(109, 208);
            this.btnRetSaleEJPer.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnRetSaleEJPer.MForeColor = System.Drawing.Color.Black;
            this.btnRetSaleEJPer.MHeight = 52;
            this.btnRetSaleEJPer.MText = "";
            this.btnRetSaleEJPer.MWidth = 85;
            this.btnRetSaleEJPer.Name = "btnRetSaleEJPer";
            this.btnRetSaleEJPer.Size = new System.Drawing.Size(85, 52);
            this.btnRetSaleEJPer.TabIndex = 9;
            this.btnRetSaleEJPer.Click += new System.EventHandler(this.OnReturnSale);
            // 
            // pnlEJZCopy
            // 
            this.pnlEJZCopy.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlEJZCopy.BackgroundImage")));
            this.pnlEJZCopy.Controls.Add(this.numerZCopyZNo);
            this.pnlEJZCopy.Controls.Add(this.btnMainEJZCopy);
            this.pnlEJZCopy.Controls.Add(this.lblZCopyZNo);
            this.pnlEJZCopy.Controls.Add(this.btnRetSaleEJZCopy);
            this.pnlEJZCopy.Controls.Add(this.btnZCopy);
            this.pnlEJZCopy.Font = new System.Drawing.Font("Lucida Console", 14F);
            this.pnlEJZCopy.Location = new System.Drawing.Point(0, 0);
            this.pnlEJZCopy.Name = "pnlEJZCopy";
            this.pnlEJZCopy.Size = new System.Drawing.Size(305, 270);
            this.pnlEJZCopy.TabIndex = 0;
            // 
            // btnMainEJZCopy
            // 
            this.btnMainEJZCopy.BackColor = System.Drawing.Color.Transparent;
            this.btnMainEJZCopy.DisableColor = System.Drawing.Color.LightGray;
            this.btnMainEJZCopy.Location = new System.Drawing.Point(10, 208);
            this.btnMainEJZCopy.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnMainEJZCopy.MForeColor = System.Drawing.Color.Black;
            this.btnMainEJZCopy.MHeight = 52;
            this.btnMainEJZCopy.MText = "";
            this.btnMainEJZCopy.MWidth = 85;
            this.btnMainEJZCopy.Name = "btnMainEJZCopy";
            this.btnMainEJZCopy.Size = new System.Drawing.Size(85, 52);
            this.btnMainEJZCopy.TabIndex = 12;
            this.btnMainEJZCopy.Click += new System.EventHandler(this.OnMainPage);
            // 
            // btnRetSaleEJZCopy
            // 
            this.btnRetSaleEJZCopy.BackColor = System.Drawing.Color.Transparent;
            this.btnRetSaleEJZCopy.DisableColor = System.Drawing.Color.LightGray;
            this.btnRetSaleEJZCopy.Location = new System.Drawing.Point(109, 209);
            this.btnRetSaleEJZCopy.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnRetSaleEJZCopy.MForeColor = System.Drawing.Color.Black;
            this.btnRetSaleEJZCopy.MHeight = 52;
            this.btnRetSaleEJZCopy.MText = "";
            this.btnRetSaleEJZCopy.MWidth = 85;
            this.btnRetSaleEJZCopy.Name = "btnRetSaleEJZCopy";
            this.btnRetSaleEJZCopy.Size = new System.Drawing.Size(85, 52);
            this.btnRetSaleEJZCopy.TabIndex = 11;
            this.btnRetSaleEJZCopy.Click += new System.EventHandler(this.OnReturnSale);
            // 
            // pnlEJSingZR
            // 
            this.pnlEJSingZR.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlEJSingZR.BackgroundImage")));
            this.pnlEJSingZR.Controls.Add(this.btnDocCopy);
            this.pnlEJSingZR.Controls.Add(this.numerSingleCopyRcptNo);
            this.pnlEJSingZR.Controls.Add(this.btnMainEJSingZR);
            this.pnlEJSingZR.Controls.Add(this.numerSingleCopyZNo);
            this.pnlEJSingZR.Controls.Add(this.btnRetSaleEJSingZR);
            this.pnlEJSingZR.Controls.Add(this.lblSingleCopyRcptNo);
            this.pnlEJSingZR.Controls.Add(this.lblSingleCopyZNo);
            this.pnlEJSingZR.Font = new System.Drawing.Font("Lucida Console", 14F);
            this.pnlEJSingZR.Location = new System.Drawing.Point(0, 0);
            this.pnlEJSingZR.Name = "pnlEJSingZR";
            this.pnlEJSingZR.Size = new System.Drawing.Size(305, 270);
            this.pnlEJSingZR.TabIndex = 1;
            // 
            // btnMainEJSingZR
            // 
            this.btnMainEJSingZR.BackColor = System.Drawing.Color.Transparent;
            this.btnMainEJSingZR.DisableColor = System.Drawing.Color.LightGray;
            this.btnMainEJSingZR.Location = new System.Drawing.Point(10, 208);
            this.btnMainEJSingZR.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnMainEJSingZR.MForeColor = System.Drawing.Color.Black;
            this.btnMainEJSingZR.MHeight = 52;
            this.btnMainEJSingZR.MText = "";
            this.btnMainEJSingZR.MWidth = 85;
            this.btnMainEJSingZR.Name = "btnMainEJSingZR";
            this.btnMainEJSingZR.Size = new System.Drawing.Size(85, 52);
            this.btnMainEJSingZR.TabIndex = 12;
            this.btnMainEJSingZR.Click += new System.EventHandler(this.OnMainPage);
            // 
            // btnRetSaleEJSingZR
            // 
            this.btnRetSaleEJSingZR.BackColor = System.Drawing.Color.Transparent;
            this.btnRetSaleEJSingZR.DisableColor = System.Drawing.Color.LightGray;
            this.btnRetSaleEJSingZR.Location = new System.Drawing.Point(109, 209);
            this.btnRetSaleEJSingZR.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnRetSaleEJSingZR.MForeColor = System.Drawing.Color.Black;
            this.btnRetSaleEJSingZR.MHeight = 52;
            this.btnRetSaleEJSingZR.MText = "";
            this.btnRetSaleEJSingZR.MWidth = 85;
            this.btnRetSaleEJSingZR.Name = "btnRetSaleEJSingZR";
            this.btnRetSaleEJSingZR.Size = new System.Drawing.Size(85, 52);
            this.btnRetSaleEJSingZR.TabIndex = 11;
            this.btnRetSaleEJSingZR.Click += new System.EventHandler(this.OnReturnSale);
            // 
            // pnlEJSingDate
            // 
            this.pnlEJSingDate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlEJSingDate.BackgroundImage")));
            this.pnlEJSingDate.Controls.Add(this.tmSingleCopyTime);
            this.pnlEJSingDate.Controls.Add(this.btnMainEJSingDate);
            this.pnlEJSingDate.Controls.Add(this.dtSingleCopyDate);
            this.pnlEJSingDate.Controls.Add(this.btnDateCopy);
            this.pnlEJSingDate.Controls.Add(this.lblSingleCopyTime);
            this.pnlEJSingDate.Controls.Add(this.lblSingleCopyDate);
            this.pnlEJSingDate.Controls.Add(this.btnRetSaleEJSingDate);
            this.pnlEJSingDate.Font = new System.Drawing.Font("Lucida Console", 14F);
            this.pnlEJSingDate.Location = new System.Drawing.Point(0, 0);
            this.pnlEJSingDate.Name = "pnlEJSingDate";
            this.pnlEJSingDate.Size = new System.Drawing.Size(305, 270);
            this.pnlEJSingDate.TabIndex = 2;
            // 
            // btnMainEJSingDate
            // 
            this.btnMainEJSingDate.BackColor = System.Drawing.Color.Transparent;
            this.btnMainEJSingDate.DisableColor = System.Drawing.Color.LightGray;
            this.btnMainEJSingDate.Location = new System.Drawing.Point(10, 208);
            this.btnMainEJSingDate.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnMainEJSingDate.MForeColor = System.Drawing.Color.Black;
            this.btnMainEJSingDate.MHeight = 52;
            this.btnMainEJSingDate.MText = "";
            this.btnMainEJSingDate.MWidth = 85;
            this.btnMainEJSingDate.Name = "btnMainEJSingDate";
            this.btnMainEJSingDate.Size = new System.Drawing.Size(85, 52);
            this.btnMainEJSingDate.TabIndex = 12;
            this.btnMainEJSingDate.Click += new System.EventHandler(this.OnMainPage);
            // 
            // btnRetSaleEJSingDate
            // 
            this.btnRetSaleEJSingDate.BackColor = System.Drawing.Color.Transparent;
            this.btnRetSaleEJSingDate.DisableColor = System.Drawing.Color.LightGray;
            this.btnRetSaleEJSingDate.Location = new System.Drawing.Point(109, 209);
            this.btnRetSaleEJSingDate.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnRetSaleEJSingDate.MForeColor = System.Drawing.Color.Black;
            this.btnRetSaleEJSingDate.MHeight = 52;
            this.btnRetSaleEJSingDate.MText = "";
            this.btnRetSaleEJSingDate.MWidth = 85;
            this.btnRetSaleEJSingDate.Name = "btnRetSaleEJSingDate";
            this.btnRetSaleEJSingDate.Size = new System.Drawing.Size(85, 52);
            this.btnRetSaleEJSingDate.TabIndex = 11;
            this.btnRetSaleEJSingDate.Click += new System.EventHandler(this.OnReturnSale);
            // 
            // pnlEJPerZR
            // 
            this.pnlEJPerZR.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlEJPerZR.BackgroundImage")));
            this.pnlEJPerZR.Controls.Add(this.numerPerCopyEndRcpt);
            this.pnlEJPerZR.Controls.Add(this.btnMainEJPerZR);
            this.pnlEJPerZR.Controls.Add(this.numerPerCopyStartRcpt);
            this.pnlEJPerZR.Controls.Add(this.lblPerCopyEndRcpt);
            this.pnlEJPerZR.Controls.Add(this.numerPerCopyEndZ);
            this.pnlEJPerZR.Controls.Add(this.btnRetSaleZR);
            this.pnlEJPerZR.Controls.Add(this.numerPerCopyStartZ);
            this.pnlEJPerZR.Controls.Add(this.btnPerZR);
            this.pnlEJPerZR.Controls.Add(this.lblPerCopyStartRcpt);
            this.pnlEJPerZR.Controls.Add(this.lblPerCopyStartZ);
            this.pnlEJPerZR.Controls.Add(this.lblPerCopyEndZ);
            this.pnlEJPerZR.Font = new System.Drawing.Font("Lucida Console", 14F);
            this.pnlEJPerZR.Location = new System.Drawing.Point(0, 0);
            this.pnlEJPerZR.Name = "pnlEJPerZR";
            this.pnlEJPerZR.Size = new System.Drawing.Size(305, 270);
            this.pnlEJPerZR.TabIndex = 4;
            // 
            // btnMainEJPerZR
            // 
            this.btnMainEJPerZR.BackColor = System.Drawing.Color.Transparent;
            this.btnMainEJPerZR.DisableColor = System.Drawing.Color.LightGray;
            this.btnMainEJPerZR.Location = new System.Drawing.Point(10, 208);
            this.btnMainEJPerZR.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnMainEJPerZR.MForeColor = System.Drawing.Color.Black;
            this.btnMainEJPerZR.MHeight = 52;
            this.btnMainEJPerZR.MText = "";
            this.btnMainEJPerZR.MWidth = 85;
            this.btnMainEJPerZR.Name = "btnMainEJPerZR";
            this.btnMainEJPerZR.Size = new System.Drawing.Size(85, 52);
            this.btnMainEJPerZR.TabIndex = 12;
            this.btnMainEJPerZR.Click += new System.EventHandler(this.OnMainPage);
            // 
            // btnRetSaleZR
            // 
            this.btnRetSaleZR.BackColor = System.Drawing.Color.Transparent;
            this.btnRetSaleZR.DisableColor = System.Drawing.Color.LightGray;
            this.btnRetSaleZR.Location = new System.Drawing.Point(109, 209);
            this.btnRetSaleZR.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnRetSaleZR.MForeColor = System.Drawing.Color.Black;
            this.btnRetSaleZR.MHeight = 52;
            this.btnRetSaleZR.MText = "";
            this.btnRetSaleZR.MWidth = 85;
            this.btnRetSaleZR.Name = "btnRetSaleZR";
            this.btnRetSaleZR.Size = new System.Drawing.Size(85, 52);
            this.btnRetSaleZR.TabIndex = 11;
            this.btnRetSaleZR.Click += new System.EventHandler(this.OnReturnSale);
            // 
            // pnlEJPerDate
            // 
            this.pnlEJPerDate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlEJPerDate.BackgroundImage")));
            this.pnlEJPerDate.Controls.Add(this.tmPerCopyEndTime);
            this.pnlEJPerDate.Controls.Add(this.btnMainEJPerDate);
            this.pnlEJPerDate.Controls.Add(this.tmPerCopyStartTime);
            this.pnlEJPerDate.Controls.Add(this.btnRetSaleEJPerDate);
            this.pnlEJPerDate.Controls.Add(this.dtPerCopyEndDate);
            this.pnlEJPerDate.Controls.Add(this.lblPerCopyEndTime);
            this.pnlEJPerDate.Controls.Add(this.dtPerCopyStartDate);
            this.pnlEJPerDate.Controls.Add(this.btnPerCopyDates);
            this.pnlEJPerDate.Controls.Add(this.lblPerCopyStartTime);
            this.pnlEJPerDate.Controls.Add(this.lblPerCopyStartDate);
            this.pnlEJPerDate.Controls.Add(this.lblPerCopyEndDate);
            this.pnlEJPerDate.Font = new System.Drawing.Font("Lucida Console", 14F);
            this.pnlEJPerDate.Location = new System.Drawing.Point(0, 0);
            this.pnlEJPerDate.Name = "pnlEJPerDate";
            this.pnlEJPerDate.Size = new System.Drawing.Size(305, 270);
            this.pnlEJPerDate.TabIndex = 4;
            // 
            // btnMainEJPerDate
            // 
            this.btnMainEJPerDate.BackColor = System.Drawing.Color.Transparent;
            this.btnMainEJPerDate.DisableColor = System.Drawing.Color.LightGray;
            this.btnMainEJPerDate.Location = new System.Drawing.Point(10, 208);
            this.btnMainEJPerDate.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnMainEJPerDate.MForeColor = System.Drawing.Color.Black;
            this.btnMainEJPerDate.MHeight = 52;
            this.btnMainEJPerDate.MText = "";
            this.btnMainEJPerDate.MWidth = 85;
            this.btnMainEJPerDate.Name = "btnMainEJPerDate";
            this.btnMainEJPerDate.Size = new System.Drawing.Size(85, 52);
            this.btnMainEJPerDate.TabIndex = 12;
            this.btnMainEJPerDate.Click += new System.EventHandler(this.OnMainPage);
            // 
            // btnRetSaleEJPerDate
            // 
            this.btnRetSaleEJPerDate.BackColor = System.Drawing.Color.Transparent;
            this.btnRetSaleEJPerDate.DisableColor = System.Drawing.Color.LightGray;
            this.btnRetSaleEJPerDate.Location = new System.Drawing.Point(109, 209);
            this.btnRetSaleEJPerDate.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnRetSaleEJPerDate.MForeColor = System.Drawing.Color.Black;
            this.btnRetSaleEJPerDate.MHeight = 52;
            this.btnRetSaleEJPerDate.MText = "";
            this.btnRetSaleEJPerDate.MWidth = 85;
            this.btnRetSaleEJPerDate.Name = "btnRetSaleEJPerDate";
            this.btnRetSaleEJPerDate.Size = new System.Drawing.Size(85, 52);
            this.btnRetSaleEJPerDate.TabIndex = 11;
            this.btnRetSaleEJPerDate.Click += new System.EventHandler(this.OnReturnSale);
            // 
            // CashRegisterInput
            // 
            this.ClientSize = new System.Drawing.Size(305, 271);
            this.ControlBox = false;
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlEJPerDaily);
            this.Controls.Add(this.pnlEJPerDate);
            this.Controls.Add(this.pnlEJPerZR);
            this.Controls.Add(this.pnlEJSingDate);
            this.Controls.Add(this.pnlEJSingZR);
            this.Controls.Add(this.pnlEJZCopy);
            this.Controls.Add(this.pnlEJPeriodic);
            this.Controls.Add(this.pnlEJSingle);
            this.Controls.Add(this.pnlFMDate);
            this.Controls.Add(this.pnlFMZZ);
            this.Name = "CashRegisterInput";
            this.pnlEJPerDaily.ResumeLayout(false);
            this.pnlEJPerDaily.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numerZCopyZNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerSingleCopyRcptNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerSingleCopyZNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerPerCopyEndRcpt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerPerCopyStartRcpt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerPerCopyEndZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerPerCopyStartZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerFMEndZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerFMStartZ)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.pnlFMZZ.ResumeLayout(false);
            this.pnlFMZZ.PerformLayout();
            this.pnlFMDate.ResumeLayout(false);
            this.pnlFMDate.PerformLayout();
            this.pnlEJSingle.ResumeLayout(false);
            this.pnlEJPeriodic.ResumeLayout(false);
            this.pnlEJZCopy.ResumeLayout(false);
            this.pnlEJZCopy.PerformLayout();
            this.pnlEJSingZR.ResumeLayout(false);
            this.pnlEJSingZR.PerformLayout();
            this.pnlEJSingDate.ResumeLayout(false);
            this.pnlEJSingDate.PerformLayout();
            this.pnlEJPerZR.ResumeLayout(false);
            this.pnlEJPerZR.PerformLayout();
            this.pnlEJPerDate.ResumeLayout(false);
            this.pnlEJPerDate.PerformLayout();
            this.ResumeLayout(false);

        }

        private void numerFMStartZ_ValueChanged(object sender, EventArgs e)
        {
            if (numerFMEndZ.Value < numerFMStartZ.Value)
            {
                numerFMEndZ.Value = numerFMStartZ.Value;
            }
        }

        private void numerFMEndZ_ValueChanged(object sender, EventArgs e)
        {

            if (numerFMStartZ.Value > numerFMEndZ.Value)
            {
                numerFMStartZ.Value = numerFMEndZ.Value;
            }
        }

        private void dtFMStartDate_ValueChanged(object sender, EventArgs e)
        {
            if (dtFMEndDate.Value < dtFMStartDate.Value)
            {
                dtFMEndDate.Value = dtFMStartDate.Value;
            }
        }

        private void dtFMEndDate_ValueChanged(object sender, EventArgs e)
        {
            if (dtFMStartDate.Value > dtFMEndDate.Value)
            {
                dtFMStartDate.Value = dtFMEndDate.Value;
            }
        }

        private void dtPerCopyStartDate_ValueChanged(object sender, EventArgs e)
        {
            if (dtPerCopyStartDate.Value > dtPerCopyEndDate.Value)
            {
                dtPerCopyEndDate.Value = dtPerCopyStartDate.Value;
            }
        }

        private void dtPerCopyEndDate_ValueChanged(object sender, EventArgs e)
        {

            if (dtPerCopyEndDate.Value < dtPerCopyStartDate.Value)
            {
                dtPerCopyStartDate.Value = dtPerCopyEndDate.Value;
            }
        }

        private void numerPerCopyStartZ_ValueChanged(object sender, EventArgs e)
        {
            if (numerPerCopyStartZ.Value > numerPerCopyEndZ.Value)
            {
                numerPerCopyEndZ.Value = numerPerCopyStartZ.Value;
            }
        }

        private void numerPerCopyEndZ_ValueChanged(object sender, EventArgs e)
        {
            if (numerPerCopyEndZ.Value < numerPerCopyStartZ.Value)
            {
                numerPerCopyStartZ.Value = numerPerCopyEndZ.Value;
            }

        }

        #region IDisplay Members

        public event ConsumeKeyHandler ConsumeKey;

        public event EventHandler DisplayClosed;

        private String lastMsg = "";
        public Target Mode
        {
            set { }
        }

        public string LastMessage
        {
            get { return lastMsg; }
        }

        public void Pause()
        {
            //throw new NotImplementedException();
        }

        public void Play()
        {
            //throw new NotImplementedException();
        }

        public void Show(string msg)
        {
            lastMsg = msg;
            //throw new NotImplementedException();
        }

        public void Show(string msg, object arg0)
        {
            //throw new NotImplementedException();
        }

        public void Show(string msg, params object[] args)
        {
            //throw new NotImplementedException();
        }

        public void Show(IConfirm error)
        {
            timerMain.Stop();
            try
            {
                if (tcpService != null)
                    tcpService.SendMessage(error.Message);
            }
            catch { }
            errorForm.Show(error.Message);
            timerMain.Start();
        }

        public void Show(IProduct p)
        {
            //throw new NotImplementedException();
        }

        public void Show(ICustomer customer)
        {
            //throw new NotImplementedException();
        }

        public void ShowSale(IFiscalItem si)
        {
            //throw new NotImplementedException();
        }

        public void ShowVoid(IFiscalItem vi)
        {
            //throw new NotImplementedException();
        }

        public void Show(IAdjustment ai)
        {
            //throw new NotImplementedException();
        }

        public void Show(ISalesDocument sd)
        {
            //throw new NotImplementedException();
        }

        public void Show(ICredit credit)
        {
            //throw new NotImplementedException();
        }

        public void Show(ICurrency currency)
        {
            //throw new NotImplementedException();
        }

        public void Show(IMenuList menu)
        {
            //throw new NotImplementedException();
        }

        public void Show(string totalMsg, decimal total)
        {
            //throw new NotImplementedException();
        }

        public void Show(string firstMsg, decimal firstTotal, string secondMsg, decimal secondTotal, bool firstLine)
        {
            //throw new NotImplementedException();
        }

        public void ShowCorrect(IAdjustment adjustment)
        {
            //throw new NotImplementedException();
        }

        public void Clear()
        {
            //throw new NotImplementedException();
        }

        public void ClearError()
        {
            //throw new NotImplementedException();
        }

        public void Reset()
        {
            //throw new NotImplementedException();
        }

        public void SetBrightness(int level)
        {
            //throw new NotImplementedException();
        }

        public void Append(string msg)
        {
            //throw new NotImplementedException();
        }

        public void Append(string msg, bool moveCursor)
        {
            //throw new NotImplementedException();
        }

        public void BackSpace()
        {
            //throw new NotImplementedException();
        }

        public void CursorNext()
        {
            //throw new NotImplementedException();
        }

        public void CursorPrevious()
        {
            //throw new NotImplementedException();
        }

        public int CurrentColumn
        {
            get { return 0; }
        }

        public void ShowAdvertisement()
        {
            //throw new NotImplementedException();
        }

        public void LedOn(Leds leds)
        {
            //throw new NotImplementedException();
        }

        public void LedOff(Leds leds)
        {
            //throw new NotImplementedException();
        }

        public bool IsLedOn(Leds leds)
        {
            return false;
        }

        public bool Inactive
        {
            get
            {
                return true;//throw new NotImplementedException();
            }
            set
            {
                // throw new NotImplementedException();
            }
        }

        public bool IsPaused
        {
            get { return false; }
        }

        public void ChangeDocumentStatus(ISalesDocument doc, DisplayDocumentStatus de)
        {
            //throw new NotImplementedException();
        }

        public void ChangeCustomer(ICustomer c)
        {
            //throw new NotImplementedException();
        }

        public bool HasGraphKeyboard()
        {
            return false;
        }

        #endregion
        

        private void OnReturnSale(object sender, EventArgs e)
        {
            HideForm();
            pnlMain.BringToFront();
        }
        private void OnMainPage(object sender, EventArgs e)
        {
            pnlMain.BringToFront();
        }

        private void OnException(Exception ex)
        {
            if(ex is EJException)
            {
                //think about blocking exceptions
            }
            Error err = new Error(ex);

            errorForm.Show(err.Message);
        }

        private void btnZReport_Click(object sender, EventArgs e)
        {
            try
            {
                ICashier csh = cr.CurrentCashier;
                if (csh == null)
                {
                    errorForm.Show(PosMessage.CASHIER_LOGIN_REQUIRED);
                }
                else
                {
                    cr.Printer.PrintZReport();
                }
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void btnXReport_Click(object sender, EventArgs e)
        {
            try
            {
                IPrinterResponse response = null;

                response = cr.Printer.PrintXReport(true);
                if (!response.HasError)
                {
                    cr.DataConnector.SaveReport("XRAPORU", response.Detail);
                }
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void btnEJDetail_Click(object sender, EventArgs e)
        {
            try
            {
                cr.Printer.PrintEJSummary();
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void btnSaleReport_Click(object sender, EventArgs e)
        {
            try
            {
                IPrinterResponse response = null;

                response = cr.Printer.PrintRegisterReport(true);
                if (!response.HasError)
                    cr.DataConnector.SaveReport("ORAPORU", response.Detail);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void btnPrgReport_Click(object sender, EventArgs e)
        {

            try
            {
                IPrinterResponse response = null;

                response = cr.Printer.PrintProgramReport(true);
                if (!response.HasError)
                    cr.DataConnector.SaveReport("PRAPORU", response.Detail);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void btnFMBwZ_Click(object sender, EventArgs e)
        {
            try
            {
                IPrinterResponse response = null;

                response = cr.Printer.PrintPeriodicReport((int)numerFMStartZ.Value,
                                                          (int)numerFMEndZ.Value,
                                                           true);
                if (!response.HasError)
                    cr.DataConnector.SaveReport("MBRAPORU", response.Detail);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void btnFMBwDates_Click(object sender, EventArgs e)
        {

            try
            {
                IPrinterResponse response = null;

                response = cr.Printer.PrintPeriodicReport(dtFMStartDate.Value,
                                                          dtFMEndDate.Value,
                                                          true);
                if (!response.HasError)
                    cr.DataConnector.SaveReport("MBRAPORU", response.Detail);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void btnZCopy_Click(object sender, EventArgs e)
        {
            try
            {
                IPrinterResponse response = cr.Printer.PrintEJZReport((int)numerZCopyZNo.Value);
            }
            catch (Exception ex) 
            { 
                OnException(ex); 
            }
        }

        private void btnDocCopy_Click(object sender, EventArgs e)
        {
            try
            {
                cr.Printer.PrintEJDocument((int)numerSingleCopyZNo.Value,
                                           (int)numerSingleCopyRcptNo.Value,
                                           true);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }

        }

        private void btnDateCopy_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dtVal;
                dtVal = new DateTime(dtSingleCopyDate.Value.Year,
                                     dtSingleCopyDate.Value.Month,
                                     dtSingleCopyDate.Value.Day,
                                     tmSingleCopyTime.Value.Hour,
                                     tmSingleCopyTime.Value.Minute,
                                     0);

                cr.Printer.PrintEJDocument(dtVal, true);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }


        private void btnPerZR_Click(object sender, EventArgs e)
        {

            try
            {
                cr.Printer.PrintEJPeriodic((int)numerPerCopyStartZ.Value,
                                           (int)numerPerCopyStartRcpt.Value,
                                           (int)numerPerCopyEndZ.Value,
                                           (int)numerPerCopyEndRcpt.Value,
                                           true);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }


        private void btnPerCopyDates_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dtValStart;
                DateTime dtValEnd;
                dtValStart = new DateTime(dtPerCopyStartDate.Value.Year,
                                     dtPerCopyStartDate.Value.Month,
                                     dtPerCopyStartDate.Value.Day,
                                     tmPerCopyStartTime.Value.Hour,
                                     tmPerCopyStartTime.Value.Minute,
                                     0);

                dtValEnd = new DateTime(dtPerCopyEndDate.Value.Year,
                                     dtPerCopyEndDate.Value.Month,
                                     dtPerCopyEndDate.Value.Day,
                                     tmPerCopyEndTime.Value.Hour,
                                     tmPerCopyEndTime.Value.Minute,
                                     0);
                cr.Printer.PrintEJPeriodic(dtValStart, dtValEnd, true);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }


        private void btnEJDaily_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dtValStart;
                DateTime dtValEnd;
                dtValStart = new DateTime(dtEJPerDailyDate.Value.Year,
                                     dtEJPerDailyDate.Value.Month,
                                     dtEJPerDailyDate.Value.Day,
                                     0,
                                     0,
                                     0);

                dtValEnd = new DateTime(dtEJPerDailyDate.Value.Year,
                                     dtEJPerDailyDate.Value.Month,
                                     dtEJPerDailyDate.Value.Day,
                                     23,
                                     59,
                                     0);
                cr.Printer.PrintEJPeriodic(dtValStart, dtValEnd, true);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }

        }

        private void numeric_Enter(object sender, EventArgs e)
        {
            NumericUpDown nu = (NumericUpDown)sender;
            NumericInput nk = new NumericInput(nu);

            int locX = this.Location.X + nu.Location.X + nu.Width + 10;
            int locY = this.Location.Y + nu.Location.Y;

            nk.Location = new System.Drawing.Point(locX, locY);
            nk.StartPosition = FormStartPosition.Manual;

            nk.ShowDialog();

        }

        private void timePicker_Enter(object sender, EventArgs e)
        {
            TimePicker tp = (TimePicker)sender;
            TimeInput ti = new TimeInput(tp);

            int locX = this.Location.X + tp.Location.X + tp.Width + 10;
            int locY = this.Location.Y + tp.Location.Y;

            ti.Location = new System.Drawing.Point(locX, locY);
            ti.StartPosition = FormStartPosition.Manual;

            ti.ShowDialog();

        }

        private void btnFMZZ_Click(object sender, EventArgs e)
        {
            pnlFMZZ.BringToFront();
        }

        private void btnFMDate_Click(object sender, EventArgs e)
        {
            pnlFMDate.BringToFront();
        }

        private void btnEJSingle_Click(object sender, EventArgs e)
        {
            pnlEJSingle.BringToFront();
        }

        private void btnEJPeriodic_Click(object sender, EventArgs e)
        {
            pnlEJPeriodic.BringToFront();
        }

        private void btnEJZCopy_Click(object sender, EventArgs e)
        {
            pnlEJZCopy.BringToFront();
        }

        private void btnEJSingByZR_Click(object sender, EventArgs e)
        {
            pnlEJSingZR.BringToFront();
        }

        private void btnEJSingDate_Click(object sender, EventArgs e)
        {
            pnlEJSingDate.BringToFront();
        }

        private void btnEJPerDaily_Click(object sender, EventArgs e)
        {
            pnlEJPerDaily.BringToFront();
        }

        private void btnEJPerDate_Click(object sender, EventArgs e)
        {
            pnlEJPerDate.BringToFront();
        }

        private void btnEJPerZR_Click(object sender, EventArgs e)
        {
            pnlEJPerZR.BringToFront();
        }



        #region IDisplay Members


        public bool HasAttribute(DisplayAttribute attribute)
        {
            return false;
        }

        public void Show(IFiscalItem fi)
        {
            //throw new Exception("The method or operation is not implemented.");
        }


        public void ShowTableContent(ISalesDocument document, decimal totalAdjAmount)
        {

        }

        public void ClearTableContent()
        {
        }

        #endregion
    }
}
