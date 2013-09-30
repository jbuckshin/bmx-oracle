using System;
using System.Data;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility.Providers.Database;

namespace Inedo.BuildMasterExtensions.Oracle
{
    /// <summary>
    /// Represents an Oracle change script.
    /// </summary>
    [Serializable]
    public sealed class OracleChangeScript : ChangeScript
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleChangeScript"/> class.
        /// </summary>
        /// <param name="dr">A DataRow used to initialize the change script properties.</param>
        public OracleChangeScript(DataRow dr)
            : base(
                Convert.ToInt64(dr["Numeric_Release_Number"]),
                Convert.ToInt32(dr["Script_Id"]),
                (string)dr["Script_Name"],
                (DateTime)dr["Executed_Date"],
                Domains.YN.Yes.Equals(dr["Success_Indicator"]))
        { }
    }
}
