// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Fills the scroll down menus in configuration and visualization.
// SPECIAL NOTES : Reads the information to the scroll down items from a static file.
// ===============================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class ScrollViewAdapter : MonoBehaviour
{
    /// <summary>
    /// Pieces images.
    /// </summary>
    public Texture2D[] availableIcons;

    /// <summary>
    /// Piece item prefab.
    /// </summary>
    public RectTransform prefab;

    /// <summary>
    /// List of piece items.
    /// </summary>
    public List<ExampleItemModel> pieces = new List<ExampleItemModel>();

    /// <summary>
    /// Scrolldown list scrollview.
    /// </summary>
    public ScrollRect scrollView;

    /// <summary>
    /// Scrolldown content.
    /// </summary>
    public RectTransform content;

    /// <summary>
    /// File to read the pieces information from.
    /// </summary>
    public string fileName;

    /// <summary>
    /// List of views in the scroll down menu.
    /// </summary>
    List<ExampleItemView> views = new List<ExampleItemView>();

    /// <summary>
    /// Communication with the main script.
    /// </summary>
    public AppMainScript appMainScript;

    /// <summary>
    /// Unity's start function.
    /// </summary>
    public void FillList()
    {
        pieces.Clear();
        FetchItemModelsFromFile();
        OnFetchedNewModels(pieces);
    }

    /// <summary>
    /// Creates the item in the scroll down for each existing piece.
    /// </summary>
    /// <param name="piecesList">List of existing pieces.</param>
    void OnFetchedNewModels(List<ExampleItemModel> piecesList)
    {
        foreach(var model in piecesList)
        {
            var instance = Instantiate(prefab.gameObject) as GameObject;
            instance.transform.SetParent(content, false);
            var view = InitializeItemView(instance, model);
            views.Add(view);
        }
        appMainScript.getEndOfScrolldown();
    }

    /// <summary>
    /// Fetch the pieces model from file.
    /// </summary>
    void FetchItemModelsFromFile()
    {
        var textFile = Resources.Load<TextAsset>(fileName);
        string[] fileLines = textFile.text.Split('\n');
        for (int i = 0; i < fileLines.Length; i++)
        {
            string[] pieceInfo = fileLines[i].Split(';');
            string qtd = "0";
            if(pieceInfo.Length == 4)
            {
                qtd = pieceInfo[3];
            }
            ExampleItemModel piece = new ExampleItemModel(pieceInfo[0], pieceInfo[1], pieceInfo[2], qtd);
            pieces.Add(piece);
            appMainScript.addToListOfPiecesToFetch(pieceInfo[0], System.Int32.Parse(qtd));
        }
    }

    /// <summary>
    /// Initialize the item view in the scrolldown.
    /// Set the pieces information.
    /// </summary>
    /// <param name="viewGameObject">Gameobject to set.</param>
    /// <param name="model">Piece model</param>
    /// <returns></returns>
    ExampleItemView InitializeItemView(GameObject viewGameObject, ExampleItemModel model)
    {
        ExampleItemView view = new ExampleItemView(viewGameObject.transform);

        view.reference.text = model.reference;
        view.piece_name.text = model.piece_name;
        view.localization.text = model.localization;
        view.quantity.text = model.quantity;
        return view;

    }

    /// <summary>
    /// Set the file to read from.
    /// </summary>
    /// <param name="file_name">File name.</param>
    public void setFileName(string file_name)
    {
        fileName = file_name;
    }

    /// <summary>
    /// Class for the item views.
    /// </summary>
    public class ExampleItemView
    {
        /// <summary>
        /// Item texts.
        /// </summary>
        public TMP_Text reference, localization, piece_name, quantity;

        /// <summary>
        /// Item constructor.
        /// </summary>
        /// <param name="rootView">Item hierarchy location.</param>
        public ExampleItemView(Transform rootView)
        {
            reference = rootView.Find("ReferenceText").GetComponent<TMP_Text>();
            piece_name = rootView.Find("NameText").GetComponent<TMP_Text>();
            localization = rootView.Find("LocalizationText").GetComponent<TMP_Text>();
            quantity = rootView.Find("QuantityText").GetComponent<TMP_Text>();
        }
    }

    /// <summary>
    /// Class for the pieces model.
    /// </summary>
    public class ExampleItemModel
    {
        /// <summary>
        /// Pieces information.
        /// </summary>
        public string reference, localization, piece_name, quantity;

        /// <summary>
        /// Piece constructor.
        /// </summary>
        /// <param name="refe">Piece reference.</param>
        /// <param name="name">Piece name.</param>
        /// <param name="loc">Piece location.</param>
        /// <param name="qtd">Piece quantity.</param>
        public ExampleItemModel(string refe, string name, string loc, string qtd)
        {
            reference = refe;
            localization = loc;
            piece_name = name;
            quantity = "Qtd. " + qtd;
        }
    }
}