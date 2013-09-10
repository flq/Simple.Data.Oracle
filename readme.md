
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

#Schema Configuration
The default schema can be configured by adding an application setting with the key "Simple.Data.Oracle.Schema".

	<appSettings>
		<add key="Simple.Data.Oracle.Schema" value="CUSTOMSCHEMA" />    
	</appSettings>

Advanced customization of the schema can be configured by creating a class the implements ISchemaConfiguration in an assembly named Simple.Data.*

	[Export(typeof(ISchemaConfiguration))]
	public class MyCustomSchemaConfiguration : ISchemaConfiguration
	{
		public string Schema { get { return "ANYTHINGYOUWANT"; } }
	}

#Tests

Tests run against an Oracle 11g XE installation with the pre-installed hr user activated 

    alter user hr account unlock identified by hr

The following columns should be  added to the regions table 

    alter table regions add RegionUid RAW(16)
    alter table regions add CreateDate DATE default sysdate

The following user should be added

    -- USER SQL
	CREATE USER hr_other IDENTIFIED BY hr_other
	DEFAULT TABLESPACE USERS
	TEMPORARY TABLESPACE TEMP
	ACCOUNT UNLOCK;

	-- ROLES
	GRANT CONNECT TO hr_other;

	grant select on HR.REGIONS to hr_other;
	grant select on HR.DEPARTMENTS to hr_other;
	grant select on HR.EMPLOYEES to hr_other;
	grant select on HR.JOB_HISTORY to hr_other;
	grant select on HR.JOBS to hr_other;
	grant select on HR.LOCATIONS to hr_other;

and the following packages / procedures should be added

    create or replace
    package Department as 
      Function department_count return number;
      Function manager_of_department(dept_name IN VARCHAR2) return VARCHAR2;
      Procedure Manager_And_Count(dept_name IN VARCHAR2, P_MANAGER OUT VARCHAR2, P_COUNT OUT NUMBER);
    END Department;
    
    create or replace
    PACKAGE BODY  Department AS 
    FUNCTION department_count
       RETURN NUMBER IS dept_count NUMBER; 
    BEGIN 
       SELECT count(department_id) 
          INTO dept_count 
          FROM hr.departments; 
       RETURN(dept_count); 
    END; 
    FUNCTION manager_of_department(dept_name In VARCHAR2) 
       RETURN VARCHAR2 IS 
          dept_manager VARCHAR2(256); 
       BEGIN 
          SELECT employees.last_name 
             INTO dept_manager 
             FROM departments, employees
             where departments.department_name = dept_name and employees.employee_id = departments.manager_id; 
          RETURN(dept_manager); 
       END; 
    PROCEDURE Manager_And_Count(dept_name IN VARCHAR2, P_MANAGER OUT VARCHAR2, P_COUNT OUT NUMBER) IS 
       BEGIN 
          SELECT employees.last_name 
             INTO P_MANAGER
             FROM departments, employees
             where departments.department_name = dept_name and employees.employee_id = departments.manager_id; 
          SELECT count(employees.employee_id) 
             into P_COUNT
             FROM departments, employees
             where departments.department_name = dept_name and employees.department_id = departments.department_id; 
       
       END; 
    END Department;
    
    create or replace function Employee_Count_Department(dept_name VARCHAR2)
        RETURN NUMBER IS emp_count NUMBER;
    BEGIN
          SELECT count(employees.employee_id) 
             into emp_count
             FROM departments, employees
             where departments.department_name = dept_name and employees.department_id = departments.department_id; 
        RETURN(emp_count);
    END;
    
The connectstring can be found in the "OracleconnectivityContext". It expects a tnsnames entry in the tnsnames.ora file of your choice and that you gave password hr to the user hr.

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

