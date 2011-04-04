Since the Oracle access dll from Microsoft is pretty much deprecated you should use the ODAC stuff from Oracle. 
These days it is xcopy deployable and you obtain it here : http://www.oracle.com/technetwork/database/windows/downloads/index.html
Follow the installation instructions. This solution expects the .NET dll (Oracle.DataAccess.dll) in your lib folder, which isn't available through source control
since it needs the native backend of the client. Add the native backend to your PATH, as described in the installation instructions

Tests run against an XE installation with the pre-installed hr user activated. The connectstring can be found in the 
"OracleconnectivityContext". It expects a tnsnames entry in the tnsnames.ora file of your choice and that you gave password hr to the user hr.