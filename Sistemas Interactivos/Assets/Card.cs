using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCard : MonoBehaviour
{
    public TMP_Text nameLabel;
    public TMP_Text speciesLabel;
    public RawImage characterImage;

    public void SetData(string name, string species, Texture2D texture)
    {
        nameLabel.text = "Name: " + name;
        speciesLabel.text = "Species: " + species;
        characterImage.texture = texture;
    }

    public void Clear()
    {
        nameLabel.text = "Loading...";
        speciesLabel.text = "Loading...";
        characterImage.texture = null;
    }
}