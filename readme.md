
#Oracle DataAccess

Since the Oracle access dll from Microsoft is pretty much deprecated you should use the ODAC stuff from Oracle. 
These days it is xcopy deployable and you obtain it here : http://www.oracle.com/technetwork/database/windows/downloads/index.html

Don't forget to download the correct bitness for your app (32-bit, 64-bit).

Follow the installation instructions. This solution expects the .NET dll (Oracle.DataAccess.dll) in your lib folder, which isn't available through source control
since it needs the native backend of the client. Add the native backend to your PATH, as described in the installation instructions. 

The .NET dll. is then found under %INSTALL%\odp.net\bin\4 - copy it into the lib folder.

#Devart Provider
The solution is prepared to be compiled against the devart Oracle provider thanks to input from *Vagif Abilov*. For this some hokery-pokery is done 
in the solution and project files. You can switch from Debug/Release configurations to DevartDebug/DevartRelease. The changes in the config are

- Definition of a compile flag **DEVART**
- Referencing the corresponding devart assemblies which need to be in the lib folder

#Tests

Tests run against an XE installation with the pre-installed hr user activated. The connectstring can be found in the 
"OracleconnectivityContext". It expects a tnsnames entry in the tnsnames.ora file of your choice and that you gave password hr to the user hr.

#TnsNames

The tnsnames entry on my machine looks like this:

<pre>
XE =
  (DESCRIPTION =
    (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521))
    (CONNECT_DATA =
      (SERVER = DEDICATED)
      (SERVICE_NAME = XE)
    )
  )
</pre>

You can specify your own location for the tnsnames.ora file by 

- firing up regedit, 
- finding HKLM\Software\Oracle\KEY_Homename, where Homename is the name you provided during installation
- adding the value TNS_ADMIN = "Path to your tnsnames.ora file"