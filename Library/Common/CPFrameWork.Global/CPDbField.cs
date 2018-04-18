using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPFrameWork.Global
{
   public  class CPDbField
    {
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsPK { get; set; }
        public CPEnum.FieldValueTypeEnum ValueType { get; set; }
        public int ValueLength { get; set; }
        public bool IsAllowNull { get; set; }
    }
    public class CPDbTable
    {
        public string TableName { get; set; }
        public string PKNames { get; set; }
    }
}
