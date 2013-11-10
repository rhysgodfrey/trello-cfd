using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrelloCFD.Models;

namespace TrelloCFD.Models
{
    public class DiagramsModel
    {
        public DiagramsModel(CumulativeFlowModel cards, CumulativeFlowModel points = null)
        {
            Cards = cards;
            Points = points;
        }

        public CumulativeFlowModel Cards { get; private set; }
        public CumulativeFlowModel Points { get; private set; }
    }
}