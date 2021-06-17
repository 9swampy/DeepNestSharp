namespace DeepNestLib
{
    using System.Collections.Generic;

    public class DataInfo
    {
        public int index;
        public List<NFP> sheets;
        public int[] sheetids;
        public int[] sheetsources;
        public List<List<NFP>> sheetchildren;
        public PopulationItem individual;
        public SvgNestConfig config;
        public int[] ids;
        public int[] sources;
        public List<List<NFP>> children;

        // ipcRenderer.send('background-start', { index: i, sheets: sheets, sheetids: sheetids, sheetsources: sheetsources, sheetchildren: sheetchildren,
        // individual: GA.population[i], config: config, ids: ids, sources: sources, children: children});
    }
}
