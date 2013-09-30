using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Oracle
{
    /// <summary>
    /// Custom editor for the Oracle database provider.
    /// </summary>
    internal sealed class OracleDatabaseProviderEditor : ProviderEditorBase
    {
        private ValidatingTextBox txtConnectionString;

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based
        /// implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            // txtConnectionString
            txtConnectionString = new ValidatingTextBox()
            {
                Width = 300,
                Required = true
            };

            CUtil.Add(this,
                new FormFieldGroup(
                    "Connection String",
                    "The connection string to the Oracle database. The standard format for this is:<br /><br />"
                    + "<em>DATA SOURCE=myServerAddress; USER ID=myUsername; PASSWORD=myPassword</em>",
                    false,
                    new StandardFormField(string.Empty, txtConnectionString)
                )
            );

            base.CreateChildControls();
        }

        public override void BindToForm(ProviderBase extension)
        {
            EnsureChildControls();

            var mysql = (OracleDatabaseProvider)extension;
            txtConnectionString.Text = mysql.ConnectionString;
        }

        public override ProviderBase CreateFromForm()
        {
            EnsureChildControls();

            return new OracleDatabaseProvider
            {
                ConnectionString = txtConnectionString.Text
            };
        }
    }
}
