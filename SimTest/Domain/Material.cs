using System;
using System.Collections.Generic;
using System.Text;

namespace SimTest.Domain
{
    public class Material
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParrentMaterialID { get; set; }
        public Material ParrentMaterial { get; set; }
        public List<Material> Materials { get; set; }
        public long AssemblyDuration { get; set; }
        public int Quantity { get; set; }
        public bool IsReady { get; set; } = false;
    }
}
