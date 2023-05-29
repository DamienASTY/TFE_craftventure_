using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMB : MonoBehaviour
{
    Chunk owner;

    public void SetOwner(Chunk o)
    {
        owner = o;
        InvokeRepeating("SaveProgress", 10, 1000);
    }

    // Méthode appelée par le propriétaire du chunk pour définir le propriétaire de ce script
    // et déclencher une sauvegarde périodique du chunk

    public IEnumerator HealBlock(Vector3 bpos)
    {
        yield return new WaitForSeconds(3);

        // Récupère les coordonnées entières du bloc à soigner
        int x = (int)bpos.x;
        int y = (int)bpos.y;
        int z = (int)bpos.z;

        // Réinitialise le bloc à sa configuration par défaut s'il n'est pas de type AIR
        if (owner.chunkData[x, y, z].bType != Block.BlockType.AIR)
            owner.chunkData[x, y, z].Reset();
    }

    // Coroutine pour la chute d'un bloc vers le bas

    public IEnumerator Drop(Block b, Block.BlockType bt, int maxdrop)
    {
        Block thisBlock = b;
        Block prevBlock = null;

        for (int i = 0; i < maxdrop; i++)
        {
            Block.BlockType previousType = thisBlock.bType;

            // Change le type du bloc en cours de chute
            if (previousType != bt)
                thisBlock.SetType(bt);

            // Rétablit le type du bloc précédent
            if (prevBlock != null)
                prevBlock.SetType(previousType);

            prevBlock = thisBlock;
            b.owner.Redraw();

            yield return new WaitForSeconds(0.2f);

            Vector3 pos = thisBlock.position;

            // Récupère le bloc situé juste en dessous
            thisBlock = thisBlock.GetBlock((int)pos.x, (int)pos.y - 1, (int)pos.z);

            // Arrête la chute si le bloc en dessous est solide
            if (thisBlock.isSolid)
            {
                yield break;
            }
        }
    }

    // Coroutine pour la Génération d'un fluide

    public IEnumerator Flow(Block b, Block.BlockType bt, int strength, int maxsize)
    {
        // Réduit la force du fluide à chaque nouveau bloc créé
        if (maxsize <= 0) yield break;
        if (b == null) yield break;
        if (strength <= 0) yield break;
        if (b.bType != Block.BlockType.AIR) yield break;

        // Change le type du bloc en cours de propagation et initialise sa santé courante
        b.SetType(bt);
        b.currentHealth = strength;
        b.owner.Redraw();

        yield return new WaitForSeconds(1);

        int x = (int)b.position.x;
        int y = (int)b.position.y;
        int z = (int)b.position.z;

        // Génération vers le bas si un bloc d'air est en dessous
        Block below = b.GetBlock(x, y - 1, z);
        if (below != null && below.bType == Block.BlockType.AIR)
        {
            StartCoroutine(Flow(b.GetBlock(x, y - 1, z), bt, strength, --maxsize));
            yield break;
        }
        else // Génération vers l'extérieur
        {
            --strength;
            --maxsize;

            // Génération vers la gauche
            StartCoroutine(Flow(b.GetBlock(x - 1, y, z), bt, strength, maxsize));
            yield return null;

            // Génération vers la droite
            StartCoroutine(Flow(b.GetBlock(x + 1, y, z), bt, strength, maxsize));
            yield return null;

            // Génération vers l'avant
            StartCoroutine(Flow(b.GetBlock(x, y, z + 1), bt, strength, maxsize));
            yield return null;

            // Génération vers l'arrière
            StartCoroutine(Flow(b.GetBlock(x, y, z - 1), bt, strength, maxsize));
            yield return null;
        }
    }

    // Méthode appelée périodiquement pour sauvegarder le progrès du chunk si des modifications ont été apportées

    void SaveProgress()
    {
        if (owner.changed)
        {
            owner.Save();
            owner.changed = false;
        }
    }
}