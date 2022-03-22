using System.Linq;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Types;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly
{
    public sealed partial class AssemblyTree
    {
        #region Properties

        private string _searchText = "";
        private AssemblyType _searchType = AssemblyType.Runtime | AssemblyType.Editor | AssemblyType.Test;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value)
                    return;

                _searchText = value;
                Reload();
            }
        }

        public AssemblyType SearchType
        {
            get => _searchType;
            set
            {
                if (_searchType == value)
                    return;

                _searchType = value;
                Reload();
            }
        }
        
        public bool HasUnsavedChanges => _projectAssemblies.Any(x => x.IsDirty);

        #endregion

        public void Refresh()
        {
            UpdateAssemblies();
            Reload();
        }

        public void ApplyChanges(bool useGuid)
        {
            foreach (var projectAssembly in _projectAssemblies)
            {
                projectAssembly.Store(useGuid);
            }
            
            Refresh();
        }

        public void DiscardChanges()
        {
            foreach (var projectAssembly in _projectAssemblies)
            {
                projectAssembly.Revert();
            }
            
            Refresh();
        }
    }
}