using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class LesManager : MonoBehaviour
{
    [SerializeField] private GameObject lessenPanel;
    [SerializeField] private GameObject lesButtonPrefab;
    [SerializeField] private Transform lesButtonContainer;
    [SerializeField] private TMP_Dropdown leerlingDropdown;
    
    private Leerling selectedLeerling;
    private Dictionary<string, Color> leerlingKleuren = new Dictionary<string, Color>();

    public void InitializeLeerlingDropdown()
    {
        if (RijschoolApp.instance.selectedRijschool == null) return;

        leerlingDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        options.Add(new TMP_Dropdown.OptionData("Selecteer leerling"));
        
        foreach (var leerling in RijschoolApp.instance.selectedRijschool.leerlingen)
        {
            options.Add(new TMP_Dropdown.OptionData(leerling.naam));
        }
        
        leerlingDropdown.AddOptions(options);
    }

    public void OnLeerlingSelected(int index)
    {
        if (index == 0)
        {
            selectedLeerling = null;
            return;
        }

        selectedLeerling = RijschoolApp.instance.selectedRijschool.leerlingen[index - 1];
    }

    public void AssignLeerlingToLes(Les les)
    {
        if (selectedLeerling == null) return;

        les.leerlingId = selectedLeerling.naam;
        les.leerlingNaam = selectedLeerling.naam;
        
        // Update UI and save changes
        RijschoolApp.instance.UpdateRijschool(RijschoolApp.instance.selectedRijschool);
    }

    public Color GetLeerlingColor(string leerlingNaam)
    {
        if (string.IsNullOrEmpty(leerlingNaam)) return Color.white;
        
        int index = RijschoolApp.instance.selectedRijschool.leerlingen
            .FindIndex(l => l.naam == leerlingNaam);
            
        return RijschoolApp.instance.GetLeerlingColor(index);
    }
} 