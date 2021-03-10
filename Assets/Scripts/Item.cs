using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "New Item/item")]
public class Item : ScriptableObject {

    public string itemName;
    [TextArea]
    public string itemDesc;//아이템 설명
    public ItemType itemType;
    public Sprite itemImage;
    public GameObject itemPrefab;

    public GameObject kitPrefab; //키트 프리펩
    public GameObject kitPreviewPrefab;//키트 프리뷰 프리펩

    public string weaponType;

    public enum ItemType
    {
        Equipment,
        Used,
        Ingredient,
        Kit,
        ETC
    }

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
