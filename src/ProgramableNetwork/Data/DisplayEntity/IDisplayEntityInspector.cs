using ProgramableNetwork.Data.DisplayEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork.Ui.DisplayEntity
{
    public interface IDisplayEntityInspector
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="panel"></param>
        /// <returns>Clearing function</returns>
        Action Create(DisplayEntityInspector panel);
    }
}
