using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestStandApp
{
    class ComModel
    {
       private string id { get; set; }
       private string data { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ComModel model &&
                   id == model.id &&
                   data == model.data;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, data);
        }


    }
}
