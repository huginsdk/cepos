## CEPOS Fiscal Sales Application

Cepos can be run with Hugin FPU or GUI Printer.

### GUI Printer

#### 1-) GUI Printer Activate

	The "PrinterGUIActive" key of value in the file that  "/Hugin.POS/Properties/App.config" must be "1" to run Cepos 
	with GUI Printer.
	
#### 2-) Set IntegrationMode

	Cepos has 3 integration mode.

	1- Datecs
	2- 5200
	3- S60D Keyboard

	You can change the "IntegrationMode" key of value in the file that "/Hugin.POS/Properties/App.config".


#### 3-) Login Cepos

	Default cashier name is "KASİYER 1" and its password "000000" to login Cepos. All cashier names and passwords
	are in the file that "/Hugin.POS/Data/KASIYER.DAT".

### Hugin FPU(Fiscal Printer Unit)

#### 1-) Hugin FPU Activate

	The "PrinterGUIActive" key of value in the file that  "/Hugin.POS/Properties/App.config" must be "0" to run Cepos
	with Hugin FPU.
	
#### 2-) Set IntegrationMode

	Cepos has 3 integration mode.

	1- Datecs
	2- 5200
	3- S60D Keyboard

	You can change the "IntegrationMode" key of value in the file that  "/Hugin.POS/Properties/App.config".

#### 3-) Set FiscalId

	You must set the "FiscalId" key of value in the file that "/Hugin.POS/Properties/App.config" as your device FiscalId.

#### 4-) Set Connection Type
	
	There are two connection type. One of the connections must be selected to pair with the device.

#### &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;4.1-) Tcp Connection

		The "PrinterAddress" key of value in the file that  "/Hugin.POS/Properties/App.config" must set ip address
		and port.


#### &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;4.2-) Serial Connection

		The "PrinterComPort" key of value in the file that "/Hugin.POS/Properties/App.config" must set serial port.

#### 5-)	Login Cepos

	Default cashier name is "KASİYER 1" and its password "000000" to login Cepos. All cashier names and passwords
	are in the file that "/Hugin.POS/Data/KASIYER.DAT".
