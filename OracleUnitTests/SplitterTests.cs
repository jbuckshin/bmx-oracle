using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inedo.BuildMasterExtensions.Oracle.UnitTests
{
    [TestClass]
    public class SplitterTests
    {
        private const string SimpleStatement = "select * from \"TestTable\"";
        private const string CreateDrop = @"create table t (number a, varchar2(10) );
drop table t;";
        private const string PlSqlBlock = @"begin
    select 1 from dual;
    select 1000 from dual;
end
";
        private const string NullPilsqlblock = "BEGIN\n  NULL;\nEND;";

        private const string CreateFunctionStatement = @"create or replace FUNCTION SCHEM.IS_XYZ (IN_PARM T1.I_NBR%TYPE) RETURN VARCHAR2 IS
  x1 VARCHAR2(1);
  x2 VARCHAR2(10);                      

BEGIN
   BEGIN
     BEGIN
       SELECT DISTINCT w
         INTO x1
         FROM e
         WHERE s = 'f';
        EXCEPTION
          WHEN NO_DATA_FOUND
            THEN x2 := 'x';
       WHEN TOO_MANY_ROWS
         THEN x2 := 'x';
        END;
     
     SELECT distinct 'Y'
     INTO x1
     FROM t p1, t p2
     WHERE p1.mynbr IN (1, 2, 33) and p1.mynbr = 'A'
       AND p2.mynbr IN (4, 44, 444, 3)
       AND(IN_PARM LIKE p1.grp_nbr
               OR x2 LIKE p1.r)
       AND(IN_PARM LIKE p2.grp_nbr
               OR x2 LIKE p2.r);

        EXCEPTION
          WHEN NO_DATA_FOUND
            THEN x1 := 'N';
   END;
   
   RETURN x1;

 END IS_XYZ;

        CREATE OR REPLACE PUBLIC SYNONYM IS_XYZ FOR SCHEM.IS_XYZ;

        GRANT EXECUTE ON SCHEM.IS_XYZ TO PUBLIC;";


        private const string CreateProcedureStatement = @"create or replace PROCEDURE ABC 
(
  i_a x.field1%TYPE,
  i_b x.field2%TYPE,
  i_c x.field3%TYPE,
  MY_CUR  OUT SYS_REFCURSOR
) 
AS 
blah VARCHAR2(1);
BEGIN
  OPEN MY_CUR FOR
  SELECT  ( pb.premium * decode(ig.bill_period, 'A', 12, 'M', 1, 'Q', 4, 'S', 2, 1)) pperiod, decode(ig.per, 'A', 'asdfasd', 'M', 'asdfa', 'Q', 'xcvb', 'S', 'erter', 'MONTHLY') f1, m.f2, m.f3, m.f4,m.f5,m.f3,m.f24,m.f44,m.f55,m.f33,m.state,m.zip,m.phone, m.f322,m.f323, mp.f323, mp.f32f54, mp.f4343, mp.efdate, mp.exate, pb.lksld, ig.name
    FROM t1 m, t2 mp, t3 gp, t4 ig, t5 pb
    WHERE m.x=mp.x
    AND ((mp.exp_date >= SYSDATE or mp.exp_date is null))
    AND mp.group = gp.group and mp.key1 = gp.key1 and mp.plan = gp.plan
    AND gp.group = pb.group and gp.plan = pb.plan
    AND gp.key1 = pb.key1
    AND ig.group = gp.group and ig.key1 = gp.key1
    AND m.x = i_1
    AND m.y = i_b 
    AND (mp.eff_date = (select max(eff_date) from t7 where x = m.x))
    AND (gp.exp_date is null or gp.exp_date >= sysdate)
    AND pb.p = (select max(p) from ptable where group = gp.group and key1 = gp.key1 and plan = gp.plan)
    ORDER by mp.exp_date desc;
END ABC;";

        private const string CreateMatViewStatement = @"CREATE MATERIALIZED VIEW SCHEM.CFEP (p1, p2, p3, p4, p5, LASTNAME, SUFFIX, p6, p7, p8, p0, p44, PHONE, PHONE_DISP, p34, ADDRESSLINE1, ADDRESSLINE2, CITY, STATE, ZIPCODE, EMAIL, p443, df, sdf, er, df, sdf, asdf, wefw, xccds)
  ORGANIZATION HEAP PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 
 NOCOMPRESS LOGGING
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE SCHEM 
  BUILD IMMEDIATE
  USING INDEX 
  REFRESH COMPLETE ON DEMAND
  USING DEFAULT LOCAL ROLLBACK SEGMENT
  USING ENFORCED CONSTRAINTS DISABLE QUERY REWRITE
  AS SELECT P.LLL_K,
       PA.ADDRESS_K,
       DECODE(EA.RTK, 'asdf','1 EXCH','asdfas', '2 EXCH') PIFF,
       UPPER(TRIM(P.FIRSTNAME)) FIRSTNAME,
       UPPER(TRIM(P.MIDDLEINITIAL)) MIDDLEINITIAL,
       UPPER(TRIM(P.LASTNAME)) LASTNAME,
       UPPER(TRIM(P.SUFFIX)) SUFFIX,
       UPPER(TRIM(P.PI)) PI,
       UPPER(TRIM(P.LONGNAME)) LONGNAME,
       UPPER(TRIM(P.SR1)) SR1,
       UPPER(TRIM(P.ID)) P_NUM,
       UPPER(TRIM(PAS.OID)) OID,
       PAS.PHONE,
       SUBSTR(RTRIM(PAS.PHONE), 1, 3)||'-'
          ||SUBSTR(RTRIM(PAS.PHONE), 4, 3)||'-'
          ||SUBSTR(RTRIM(PAS.PHONE), 7, 4) PHONE_DISP,
       UPPER(TRIM(G.GN)) AS GN,
       UPPER(TRIM(PAS.ADDRESSLINE1)) ADDRESSLINE1,
       UPPER(TRIM(PAS.ADDRESSLINE2)) ADDRESSLINE2,
       UPPER(TRIM(PAS.CITY)) CITY,
       UPPER(TRIM(PAS.STATE)) STATE,
       TRIM(PAS.ZIPCODE) ZIPCODE,
       UPPER(TRIM(PAS.EMAIL)) EMAIL,
       UPPER(TRIM(PAS.USERDEF)) ACCESS,
       DECODE(TRIM(R1.DESCRIPTION), 'a,b', '00', 'asdf', '10', 'qwer', '15', 'dfgh', '20', 'ghjk', '30', 'xcvb', '40', 'dfg', '50', '99') SEPCS,
	   UPPER(SUBSTR(DECODE(TRIM(PAS.ADDRESSLINE2), '', TRIM(PAS.ADDRESSLINE1), DECODE(TRIM(PAS.ADDRESSLINE1), TRANSLATE(TRIM(PAS.ADDRESSLINE1), '0123456789', 'XXXXXXXXXX'), TRIM(PAS.ADDRESSLINE2), DECODE(SUBSTR(TRIM(PAS.ADDRESSLINE1), 1, 4), 'P.O.', TRIM(PAS.ADDRESSLINE2), TRIM(PAS.ADDRESSLINE1)))), 1, 30)) ADDRMAP,
       DECODE(REGEXP_REPLACE(PA.STARTTIME, '[^0-9]+', ''),
              NULL, DECODE(REGEXP_REPLACE(PA.ENDTIME, '[^0-9]+', ''), NULL, NULL, SUBSTR(REGEXP_REPLACE(PA.ENDTIME, '[^0-9]+', ''), 1, LENGTH(REGEXP_REPLACE(PA.ENDTIME, '[^0-9]+', '')) - 2)||':'||SUBSTR(REGEXP_REPLACE(PA.ENDTIME, '[^0-9]+', ''), -2)||' AM'),
              SUBSTR(REGEXP_REPLACE(PA.STARTTIME, '[^0-9]+', ''), 1, LENGTH(REGEXP_REPLACE(PA.STARTTIME, '[^0-9]+', '')) - 2)||':'||SUBSTR(REGEXP_REPLACE(PA.STARTTIME, '[^0-9]+', ''), -2)||' PM') STARTTIME
FROM C.t1 P,
    C.t2 PA,
    C.t3 PAS,
    C.t4 EA,
    C.t5 PS,
    C.t6 R1,
    C.t7 GPA,
    C.t8 GA,
    C.t9 G
WHERE P.ACTIVE = 1
  AND PA.P_K = P.P_K
  AND PAS.ADDRESS_J = PA.ADDRESS_J
  AND EA.EIK_R = P.EIK_R
  AND P.SI_W = PS.SI_W
  AND PS.KS_E = R1.KS_E
  AND EXISTS (SELECT 1
              FROM C.WAS E
              WHERE E.P_K = P.P_K
                AND (TRIM(E.USERDEF_YY5) <> 'NONE'
                       OR
                     TRIM(E.USERDEF_YY5) = 'NONE' AND
                     TRIM(E.STATUS_EWK) <> 'NONE' AND
                     EXISTS (SELECT 1
                             FROM C.REFTABLE R
                             WHERE R.REFTABLE_K = E.STATUS_ASE
                               AND TRIM(R.DESCRIPTION) IN ('my desc', 'STREET'))
                    )
             )
  AND NOT EXISTS (SELECT 1
                  FROM c.EAS E,
                       c.REFTABLE R,
                       c.ESS S
                  WHERE E.P_K = P.P_K
                    AND TRIM(E.ASSIGNMENT_EIK) <> 'FIVE'
                    AND R.REFTABLE_K = E.ASSIGNMENT_EIK
                    AND TRIM(R.DESCRIPTION) IN ('SOONER', 'LATER')
                    AND S.EA_K = E.EA_K
                    AND PA.PADDR_K = S.PADDR_K
                 )
  AND EXISTS (SELECT 1
              FROM c.WIK E,
                   c.REFTABLE R
              WHERE E.P_K = P.P_K
                AND R.REFTABLE_K = E.ASSIGNMENT_EIK
                AND TRIM(R.DESCRIPTION) NOT IN (SELECT ENAME
                                                        FROM SMW))
  AND EXISTS (SELECT 1
              FROM c.PLIC LI
              WHERE LI.P_K = P.P_K
                AND LI.LICENSE = 'ASDFAS'
                AND LI.STATE = PAS.STATE)
  AND EXISTS (SELECT 1
              FROM c.EAS
              WHERE RECORDTYPE = 'Q'
                AND ACTIVE = 1
                AND P_K = P.P_K
                AND (DATE_FROM IS NOT NULL
                      OR
                      DATE_FROM IS NULL
                      AND EXISTS (SELECT 1
                                  FROM c.EAS E
                                  WHERE E.P_K = P.P_K
                                    AND TRIM(E.USERDEF_EIW5) = 'asdf'
                                    AND TRIM(E.STATUS_SDS) <> 'werqqf'
                                    AND EXISTS (SELECT 1
                                                 FROM c.REFE R
                                                 WHERE R.REFE_K = E.STATUS_EIK
                                                   AND TRIM(R.DESCRIPTION) = 'asdfasdf')))
                AND (TERM_DATE IS NULL OR TERM_DATE > SYSDATE + 30)
                AND USERDEF_EIK NOT IN (SELECT REFTABLE FROM c.REFTABLE WHERE TRIM(REF_Y)='343' AND SHORT_ORDER IN ('asdf','asdf','asdfss','sdfcxv')) )
  AND EA.RT = 'A'
  AND EA.ACTIVE = 1
  AND EA.ASK IN
        (SELECT REFTABLE FROM c.REFTABLE WHERE RE_K in ('fsdfg','sdfgsd')) --(comment)  -----  'last comment
          AND GPA.sdfg (+)= PA.sdfg AND GA.ghjg (+)= GPA.Gfghj AND G.fghjg (+)= GA.ghj
  AND TRIM(R1.DESCRIPTION) in ('df, dfg', 'sdfgdsf', 'ghfg', 'dfgdf', 'xdfgh', 'fgh', 'sdfg');

  CREATE INDEX SCHEM.C_IDX2 ON SCHEM.CFEP (CITY, STATE) 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE SCHEM ;
  CREATE INDEX SCHEM.C_IDX3 ON SCHEM.CFEP (ZIP) 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE SCHEM ;

   COMMENT ON MATERIALIZED VIEW SCHEM.CFEP  IS 'snapshot table for snapshot SCHEM.CFEP';
";

        private const string MultiTypeStatement = @"drop type MYTABTYPE;
drop type MYTYPE;

create or replace TYPE MYTYPE  AS OBJECT 
(  
lname           VARCHAR2(35) 
);

create or replace TYPE MYTABTYPE AS TABLE OF MYTYPE;
";

        private const string SimplePackageSemi = @"create or replace PACKAGE BODY my_pack AS

  PROCEDURE update_email
          (id_in VARCHAR2)
  IS
  BEGIN
    UPDATE id_tab
      SET e_mail_addr = 'asdf@asdf.com'
      WHERE id_type = 'x'
        AND id_id = id_in
		AND sfx = 3;
    
    COMMIT;
  END update_email;

END my_pack;";

        private const string SingleStatementWithSemi = @"create or replace TYPE MYTABTYPE AS TABLE OF myType;";

        private const string CreateTableWithComments = @"CREATE TABLE SCHEM.F_YEAR 
   (	PI VARCHAR2(10 BYTE) NOT NULL ENABLE, 
	YEAR NUMBER(4,0) NOT NULL ENABLE, 
	N_TYPE CHAR(1 BYTE), 
	S_NBR VARCHAR2(5 BYTE), 
	 CONSTRAINT F_YEAR_PK PRIMARY KEY (NPI, YEAR)
  USING INDEX PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE SCHEM  ENABLE
   ) SEGMENT CREATION IMMEDIATE 
  PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 
 NOCOMPRESS LOGGING
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT FLASH_CACHE DEFAULT CELL_FLASH_CACHE DEFAULT)
  TABLESPACE SCHEM;

   COMMENT ON COLUMN SCHEM.F_YEAR.YEAR IS 'my comment 1.';
   COMMENT ON COLUMN SCHEM.F_YEAR.N_TYPE IS '''1'' for yes, ''2'' for no';
   COMMENT ON COLUMN SCHEM.F_YEAR.S_NBR IS 'If N_TYPE = 2, this field contains the blagh to associate to blip';";

        private const string CreateFuncWithExtras = @"create or replace FUNCTION SCHEM.IS_SOUP (IN_NBR IN_TAB.IN_NBR%TYPE) RETURN VARCHAR2 IS
  SOUP_VAR VARCHAR2(1);
  ID_VAR VARCHAR2(10);                      
--
-- Returns Y if the yummy soup.
-- Code by Haapy Day.
-- Wrapped be HH.
--
BEGIN
   BEGIN
     BEGIN
       SELECT DISTINCT p_id
         INTO ID_VAR
         FROM i_group
         WHERE x_nbr = IN_NBR;
        EXCEPTION
          WHEN NO_DATA_FOUND
            THEN ID_VAR := 'x';
       WHEN TOO_MANY_ROWS
         THEN ID_VAR := 'x';
        END;
     
     SELECT distinct 'Y'
     INTO SOUP_VAR
     FROM p_group p1, p_group p2
     WHERE p1.p_nbr IN (2, 3, 4) and p1.p_sfx = 'A'
       AND p2.p_nbr IN (4, 6, 7, 88)
       AND(IN_NBR LIKE p1.g_nbr
               OR IN_NBR LIKE p1.g_nbr)
       AND(IN_NBR LIKE p2.g_nbr
               OR IN_NBR LIKE p2.g_nbr);

        EXCEPTION
          WHEN NO_DATA_FOUND
            THEN SOUP_VAR := 'N';
   END;
   
   RETURN SOUP_VAR;

        END IS_SOUP;

        CREATE OR REPLACE PUBLIC SYNONYM IS_SOUP FOR SCHEM.IS_SOUP;

        GRANT EXECUTE ON SCHEM.IS_SOUP TO PUBLIC;";


        
        [TestMethod]
        public void TestPilsql_CreateFuncWithExtras()
        {
            string[] statements = ScriptSplitter.Process(CreateFuncWithExtras).ToArray();
            Assert.IsTrue(3 == statements.Length);
            Assert.IsTrue(';' == statements[0].ElementAt(statements[0].Length - 1));
        }

        [TestMethod]
        public void TestPilsql_CreateTableWithComments()
        {
            string[] statements = ScriptSplitter.Process(CreateTableWithComments).ToArray();
            Assert.IsTrue(4 == statements.Length);
        }

        [TestMethod]
        public void TestPilsql_SimplePackageSemiStatement()
        {
            string[] statements = ScriptSplitter.Process(SimplePackageSemi).ToArray();
            Assert.IsTrue(1 == statements.Length);
            Assert.IsTrue(statements[0].LastIndexOf(';') == statements[0].Length-1);
        }

        [TestMethod]
        public void TestPilsql_SingleWithSemiStatement()
        {
            string[] statements = ScriptSplitter.Process(SingleStatementWithSemi).ToArray();
            Assert.IsTrue(1 == statements.Length);
            Assert.IsTrue(statements[0].IndexOf(';') == -1);
        }

        [TestMethod]
        public void TestPilsql_MultiTypeStatement()
        {
            Assert.IsTrue(4 == ScriptSplitter.Process(MultiTypeStatement).ToArray().Length);
        }

        [TestMethod]
        public void TestPilsql_CreateMatViewStatement()
        {
            Assert.IsTrue(4 == ScriptSplitter.Process(CreateMatViewStatement).ToArray().Length);
        }
        

        [TestMethod]
        public void TestPilsql_CreateFunctionStatement()
        {
            Assert.IsTrue(3 == ScriptSplitter.Process(CreateFunctionStatement).ToArray().Length);
        }
        [TestMethod]
        public void TestPilsql_CreateProcedureStatement()
        {
            string[] statements = ScriptSplitter.Process(CreateProcedureStatement).ToArray();
            Assert.IsTrue(1 == statements.Length);
            Assert.IsTrue(';' == statements[0].ElementAt(statements[0].Length - 1));
        }
        

                [TestMethod]
        public void TestPilsql_SingleStatement()
        {
            CollectionAssert.AreEqual(
                new[] { NullPilsqlblock },
                ScriptSplitter.Process(NullPilsqlblock).ToArray()
            );
        }

        [TestMethod]
        public void TestPilsql_MultipleStatement_NoDelim()
        {
            CollectionAssert.AreEqual(
                new[] { NullPilsqlblock + "\n\n" + NullPilsqlblock },
                ScriptSplitter.Process(NullPilsqlblock + "\n\n" + NullPilsqlblock).ToArray()
            );
        }

        [TestMethod]
        public void TestPilsql_MultipleStatement_SemicolonDelim()
        {
            CollectionAssert.AreEqual(
                new[] { NullPilsqlblock, NullPilsqlblock },
                ScriptSplitter.Process(NullPilsqlblock + "\n;\n" + NullPilsqlblock).ToArray()
            );
        }

        [TestMethod]
        public void TestPilsql_MultipleStatement_SlashDelim()
        {
            CollectionAssert.AreEqual(
                new[] { NullPilsqlblock, NullPilsqlblock },
                ScriptSplitter.Process(NullPilsqlblock + "\n/\n" + NullPilsqlblock).ToArray()
            );
        }

        [TestMethod]
        public void TestPilsql_MultipleStatement_ManyDelim()
        {
            CollectionAssert.AreEqual(
                new[] { NullPilsqlblock, NullPilsqlblock },
                ScriptSplitter.Process(NullPilsqlblock + "\n/\n" + "\n/\n" + "\n;\n" + "\n/\n" + NullPilsqlblock).ToArray()
            );
        }

        [TestMethod]
        public void TestSimpleStatement()
        {
            CollectionAssert.AreEqual(
                new[] { SimpleStatement },
                ScriptSplitter.Process(SimpleStatement).ToArray()
            );
        }
        [TestMethod]
        public void TestCreateDrop()
        {
            CollectionAssert.AreEqual(
                new[] { "create table t (number a, varchar2(10) )", "drop table t" },
                ScriptSplitter.Process(CreateDrop).ToArray()
            );
        }
        [TestMethod]
        public void TestPlSqlBlock()
        {
            CollectionAssert.AreEqual(
                new[] { PlSqlBlock.Replace("\r", "").Trim() },
                ScriptSplitter.Process(PlSqlBlock).ToArray()
            );
        }
        [TestMethod]
        public void TestCreateDropPlSqlBlock()
        {
            CollectionAssert.AreEqual(
                new[] { "create table t (number a, varchar2(10) )", "drop table t", PlSqlBlock.Replace("\r", "").Trim() },
                ScriptSplitter.Process(CreateDrop + "\n" + PlSqlBlock).ToArray()
            );
        }
    }
}
