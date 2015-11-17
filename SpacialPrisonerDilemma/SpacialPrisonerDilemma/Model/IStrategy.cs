using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacialPrisonerDilemma.Model
{
    public interface IStrategy
    {
        bool Decide(Cell parent, Cell opponent);
    }
}
