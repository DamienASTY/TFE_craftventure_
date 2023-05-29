using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    // Référence au joueur
    public GameObject player;

    // Matériaux utilisés pour les textures
    public Material textureAtlas;
    public Material fluidTexture;

    // Dimensions des colonnes et des chunks
    public static int columnHeight = 16;
    public static int chunkSize = 16;

    // Taille du monde
    public static int worldSize = 1;

    // Rayon du monde généré autour du joueur
    public static int radius = 3;

    // Nombre maximal de coroutines simultanées
    public static uint maxCoroutines = 1000;

    // Dictionnaire contenant tous les chunks générés
    public static Dictionary<string, Chunk> chunks;

    // Liste des chunks à supprimer
    public static List<string> toRemove = new List<string>();

    // Utilisé pour la barre de progression
    public static bool firstbuild = true;
    public Slider buildProgress;
    public int chunkCount = 0;
    int totalChunkCount = 63; // Calculé en fonction du nombre final de chunks générés

    // Caméra utilisée pour la génération des chunks
    public Camera buildCam;

    // Bouton pour passer en mode jeu
    public Button playButton;

    // Temps de début de la génération des chunks
    float startTime;

    // Position du dernier chunk généré
    public Vector3 lastbuildPos;

    // Génère le nom du chunk en fonction de sa position
    public static string BuildChunkName(Vector3 v)
    {
        return (int)v.x + "_" +
            (int)v.y + "_" +
            (int)v.z;
    }

    // Génère le nom de la colonne en fonction de sa position
    public static string BuildColumnName(Vector3 v)
    {
        return (int)v.x + "_" + (int)v.z;
    }

    // Récupère le bloc du monde à une certaine position
    public static Block GetWorldBlock(Vector3 pos)
    {
        int cx, cy, cz;

        // Calcul des coordonnées du chunk
        if (pos.x < 0)
            cx = (int)(Mathf.Round(pos.x - chunkSize) / (float)chunkSize) * chunkSize;
        else
            cx = (int)(Mathf.Round(pos.x) / (float)chunkSize) * chunkSize;

        if (pos.y < 0)
            cy = (int)(Mathf.Round(pos.y - chunkSize) / (float)chunkSize) * chunkSize;
        else
            cy = (int)(Mathf.Round(pos.y) / (float)chunkSize) * chunkSize;


        if (pos.z < 0)
            cz = (int)(Mathf.Round(pos.z - chunkSize) / (float)chunkSize) * chunkSize;
        else
            cz = (int)(Mathf.Round(pos.z) / (float)chunkSize) * chunkSize;

        // Calcul des coordonnées locales du bloc dans le chunk
        int blx = (int)Mathf.Abs((float)Mathf.Round(pos.x) - cx);
        int bly = (int)Mathf.Abs((float)Mathf.Round(pos.y) - cy);
        int blz = (int)Mathf.Abs((float)Mathf.Round(pos.z) - cz);

        // Nom du chunk correspondant à la position
        string cn = BuildChunkName(new Vector3(cx, cy, cz));
        Chunk c;
        if (chunks.TryGetValue(cn, out c))
        {
            return c.chunkData[blx, bly, blz];
        }
        else
            return null;
    }

    // Génère un chunk à une certaine position
    void BuildChunkAt(int x, int y, int z)
    {
        // Position du chunk
        Vector3 chunkPosition = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);

        // Nom du chunk correspondant à la position
        string n = BuildChunkName(chunkPosition);
        Chunk c;

        if (!chunks.TryGetValue(n, out c))
        {
            // Création du chunk
            c = new Chunk(chunkPosition, textureAtlas, fluidTexture);
            c.chunk.transform.parent = this.transform;
            c.fluid.transform.parent = this.transform;
            chunks.Add(c.chunk.name, c);
            chunkCount++;
            buildProgress.value = chunkCount / (float)totalChunkCount * 100;

            // Si tous les chunks ont été générés, désactive la caméra de génération et la barre de progression
            if (chunkCount == totalChunkCount)
            {
                firstbuild = false;
                buildCam.gameObject.SetActive(false);
                buildProgress.gameObject.SetActive(false);
                playButton.gameObject.SetActive(false);
            }
        }
    }

    // Coroutine pour la génération récursive du monde
    IEnumerator BuildRecursiveWorld(int x, int y, int z, int startrad, int rad)
    {
        int nextrad = rad - 1;

        if (rad <= 0 || y < 0 || y > columnHeight) yield break;

        BuildChunkAt(x, y, z + 1);
        yield return StartCoroutine(BuildRecursiveWorld(x, y, z + 1, rad, nextrad));
        
        
        
        BuildChunkAt(x, y, z - 1);
        yield return StartCoroutine(BuildRecursiveWorld(x, y, z - 1, rad, nextrad));

        BuildChunkAt(x - 1, y, z);
        yield return StartCoroutine(BuildRecursiveWorld(x - 1, y, z, rad, nextrad));

        BuildChunkAt(x + 1, y, z);
        yield return StartCoroutine(BuildRecursiveWorld(x + 1, y, z, rad, nextrad));

        BuildChunkAt(x, y + 1, z);
        yield return StartCoroutine(BuildRecursiveWorld(x, y + 1, z, rad, nextrad));

        BuildChunkAt(x, y - 1, z);
        yield return StartCoroutine(BuildRecursiveWorld(x, y - 1, z, rad, nextrad));

        StartCoroutine(DrawChunks());
    }

    // Coroutine pour le dessin des chunks
    IEnumerator DrawChunks()
    {
        toRemove.Clear();
        foreach (KeyValuePair<string, Chunk> c in chunks)
        {
            if (c.Value.status == Chunk.ChunkStatus.DRAW)
            {
                c.Value.DrawChunk();
            }

            // Vérifie si le chunk est trop éloigné du joueur et doit être supprimé
            if (c.Value.chunk && Vector3.Distance(player.transform.position, c.Value.chunk.transform.position) > radius * chunkSize)
                toRemove.Add(c.Key);

            yield return null;
        }
    }

    // Coroutine pour la suppression des anciens chunks
    IEnumerator RemoveOldChunks()
    {
        for (int i = 0; i < toRemove.Count; i++)
        {
            string n = toRemove[i];
            Chunk c;
            if (chunks.TryGetValue(n, out c))
            {
                Destroy(c.chunk);
                c.Save();
                chunks.Remove(n);
                yield return null;
            }
        }
    }

    // Génère les chunks autour du joueur
         public void BuildNearPlayer()
         {
             StopCoroutine("BuildRecursiveWorld");
             StartCoroutine(BuildRecursiveWorld((int)(player.transform.position.x / chunkSize),
                 (int)(player.transform.position.y / chunkSize),
                 (int)(player.transform.position.z / chunkSize),
                 radius,
                 radius));
         }
     
         // Méthode appelée au démarrage
         void Start()
         {
             Utils.genSeed();
             
             Vector3 ppos = player.transform.position;
             player.transform.position = new Vector3(ppos.x, Utils.GenerateHeight(ppos.x, ppos.z) + 8, ppos.z);
             lastbuildPos = player.transform.position;
             player.SetActive(false);
             
             firstbuild = true;
             chunks = new Dictionary<string, Chunk>();
             this.transform.position = Vector3.zero;
             this.transform.rotation = Quaternion.identity;
     
             startTime = Time.time;
             Debug.Log("Start Build");
     
             // Génère le chunk de départ
             BuildChunkAt((int)(player.transform.position.x + 100 / chunkSize),
                 (int)(player.transform.position.y + 100 / chunkSize),
                 (int)(player.transform.position.z + 100 / chunkSize));
     
             // Dessine les chunks
             StartCoroutine(DrawChunks());
     
             // Génère un monde plus grand
             StartCoroutine(BuildRecursiveWorld((int)(player.transform.position.x / chunkSize),
                 (int)(player.transform.position.y / chunkSize),
                 (int)(player.transform.position.z / chunkSize), radius, radius));
             
             // Place le joueur à une position élevée pour éviter qu'il tombe
             //player.transform.position = new Vector3(player.transform.position.x, columnHeight * chunkSize, player.transform.position.z);
         }
     
         // Méthode appelée lors du clic sur le bouton Play
         public void PlayButtonPressed()
         {
             // Génère le chunk de départ
             BuildChunkAt((int)(player.transform.position.x / chunkSize),
                 (int)(player.transform.position.y / chunkSize),
                 (int)(player.transform.position.z / chunkSize));
     
             // Dessine les chunks
             StartCoroutine(DrawChunks());
     
             // Génère un monde plus grand
             StartCoroutine(BuildRecursiveWorld((int)(player.transform.position.x / chunkSize),
                 (int)(player.transform.position.y / chunkSize),
                 (int)(player.transform.position.z / chunkSize), radius, radius));
         }
     
         // Méthode appelée à chaque frame
         void Update()
         {
             if (firstbuild) return;
     
             // Vérifie le déplacement du joueur et génère les chunks à proximité
             Vector3 movement = lastbuildPos - player.transform.position;
             if (movement.magnitude > chunkSize)
             {
                 lastbuildPos = player.transform.position;
                 BuildNearPlayer();
             }
     
             // Active le joueur et affiche le temps de génération
             if (!player.activeSelf)
             {
                 player.SetActive(true);
                 Debug.Log("Built in " + (Time.time - startTime));
                 firstbuild = false;
             }
     
             // Dessine les chunks et supprime les anciens
             StartCoroutine(DrawChunks());
             StartCoroutine(RemoveOldChunks());
         }
}
