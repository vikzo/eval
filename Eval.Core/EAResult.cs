﻿#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using Eval.Core.Models;

namespace Eval.Core
{

    public class EAResult
    {
        public IPhenotype Winner { get; set; }
        public Population EndPopulation { get; set; }
    }
}
