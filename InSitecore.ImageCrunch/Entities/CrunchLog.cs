using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSitecore.ImageCrunch.Entities
{

    [Serializable]
    public class CrunchedStats
    {
        public enum TypeofExecution
        {
            CrunchbyTreeRibbonCommand = 0,
            CrunchofSingleImageRibbonCommand = 1,
            CrunchThroughUploadImage = 2,
            CrunchThroughAttachImage = 3
        }

        public TypeofExecution typeofExecution { get; set; }
        public string JobID { get; set; }

        /// <summary>
        /// Image Size in Bytes
        /// </summary>
        public long BeforeCrunchSize { get; set; }
        /// <summary>
        /// Image Size in Bytes
        /// </summary>
        public long AfterCrunchSize { get; set; }
        /// <summary>
        /// Time Taken in MiliSeconds
        /// </summary>
        public long TimeTaken { get; set; }
        public string Database { get; set; }
        public string InitiatedBy { get; set; }
    }
}
