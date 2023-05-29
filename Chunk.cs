using System;
using UnityEngine;

[Serializable]
// Cette classe représente les données d'un bloc, stockées sous forme d'une matrice de types de blocs
class BlockData {
    public Block.BlockType[,,] matrix;

    // Constructeur par défaut
    public BlockData() { }

    // Constructeur prenant une matrice de blocs en argument et initialisant la matrice de types de blocs
    public BlockData(Block[,,] b) {
        matrix = new Block.BlockType[World.chunkSize, World.chunkSize, World.chunkSize];
        // Parcours les dimensions de la matrice de blocs
        for (int z = 0; z < World.chunkSize; z++)
            for (int y = 0; y < World.chunkSize; y++)
                for (int x = 0; x < World.chunkSize; x++) {
                    // Affecte le type de bloc correspondant à la position (x, y, z) de la matrice de types de blocs
                    matrix[x, y, z] = b[x, y, z].bType;
                }
    }
}

public class Chunk {

// Déclaration des membres de la classe Chunk avec des commentaires détaillés

    public Material cubeMaterial; // Matériau utilisé pour les blocs du chunk
    public Material fluidMaterial; // Matériau utilisé pour les fluides du chunk
    public Block[,,] chunkData; // Matrice de blocs représentant les données du chunk
    public GameObject chunk; // GameObject représentant le chunk
    public GameObject fluid; // GameObject représentant les fluides du chunk
    public enum ChunkStatus { DRAW, DONE, KEEP }; // Énumération des différents états possibles du chunk
    public ChunkStatus status; // État actuel du chunk
    public ChunkMB mb; // Composant ChunkMB attaché au chunk
    BlockData bd; // Données des blocs du chunk (sous forme d'une matrice de types de blocs)
    public bool changed = false; // Indique si le chunk a été modifié
    bool treesCreated = false; // Indique si les arbres ont été créés dans le chunk

// Construit le nom du fichier du chunk à partir de sa position
    string BuildChunkFileName(Vector3 v) {
        return Application.persistentDataPath + "/savedata/Chunk_" +
               (int)v.x + "_" +
               (int)v.y + "_" +
               (int)v.z +
               "_" + World.chunkSize +
               "_" + World.radius +
               ".dat";
    }
    bool Load() // Lecture des données à partir du fichier
    {
        /*
        string chunkFile = BuildChunkFileName(chunk.transform.position); // Construit le nom du fichier du chunk à partir de sa position
    
        if (File.Exists(chunkFile)) // Vérifie si le fichier existe
        {
            BinaryFormatter bf = new BinaryFormatter(); // Crée un objet BinaryFormatter pour la désérialisation
            FileStream file = File.Open(chunkFile, FileMode.Open); // Ouvre le fichier en mode lecture
            bd = new BlockData(); // Crée un nouvel objet BlockData pour stocker les données désérialisées
            bd = (BlockData)bf.Deserialize(file); // Désérialise les données du fichier dans l'objet bd en utilisant le BinaryFormatter
            file.Close(); // Ferme le fichier
            //Debug.Log("Loading chunk from file: " + chunkFile); // Affiche un message de débogage indiquant que le chargement du chunk à partir du fichier a été effectué avec succès
            return true; // Retourne vrai pour indiquer que le chargement du chunk à partir du fichier a réussi
        }
        */

        return false; // Retourne faux si le fichier n'existe pas ou si le chargement a échoué
    }


    public void Save() // Écriture des données dans un fichier
    {
        /*
        string chunkFile = BuildChunkFileName(chunk.transform.position); // Construit le nom du fichier du chunk à partir de sa position
    
        if (!File.Exists(chunkFile)) // Vérifie si le fichier n'existe pas
        {
            Directory.CreateDirectory(Path.GetDirectoryName(chunkFile)); // Crée le répertoire contenant le fichier
        }
    
        BinaryFormatter bf = new BinaryFormatter(); // Crée un objet BinaryFormatter pour la sérialisation
        FileStream file = File.Open(chunkFile, FileMode.OpenOrCreate); // Ouvre le fichier en mode écriture ou le crée s'il n'existe pas
        bd = new BlockData(chunkData); // Crée un nouvel objet BlockData à partir des données du chunk
        bf.Serialize(file, bd); // Sérialise l'objet bd et écrit les données dans le fichier en utilisant le BinaryFormatter
        file.Close(); // Ferme le fichier
    
        //Debug.Log("Saving chunk from file: " + chunkFile); // Affiche un message de débogage indiquant que le chunk a été sauvegardé avec succès dans le fichier
        */
    }

    
    //Met à jour les chunks
    public void UpdateChunk() {
        for (int z = 0; z < World.chunkSize; z++) // Parcourt les coordonnées z du chunk
        for (int y = 0; y < World.chunkSize; y++) // Parcourt les coordonnées y du chunk
        for (int x = 0; x < World.chunkSize; x++) { // Parcourt les coordonnées x du chunk
            if (chunkData[x, y, z].bType == Block.BlockType.SAND) { // Vérifie si le type de bloc à ces coordonnées est du sable
                mb.StartCoroutine(mb.Drop(chunkData[x, y, z], Block.BlockType.SAND, 20)); // Appelle la coroutine Drop de l'objet mb (ChunkMB) en passant le bloc, le type de bloc et une valeur de hauteur
            }
        }
    }


    void BuildChunk() {
            bool dataFromFile = false;
            dataFromFile = Load(); // Charge les données à partir d'un fichier

            chunkData = new Block[World.chunkSize, World.chunkSize, World.chunkSize]; // Initialise le tableau de données du chunk

            for (int z = 0; z < World.chunkSize; z++) { // Parcourt les coordonnées z du chunk
                for (int y = 0; y < World.chunkSize; y++) { // Parcourt les coordonnées y du chunk
                    for (int x = 0; x < World.chunkSize; x++) { // Parcourt les coordonnées x du chunk
                        Vector3 pos = new Vector3(x, y, z); // Calcule la position du bloc dans le chunk
                        int worldX = (int)(x + chunk.transform.position.x); // Calcule la position x mondiale du bloc
                        int worldY = (int)(y + chunk.transform.position.y); // Calcule la position y mondiale du bloc
                        int worldZ = (int)(z + chunk.transform.position.z); // Calcule la position z mondiale du bloc

                        if (dataFromFile) { // Si les données ont été chargées à partir d'un fichier
                            chunkData[x, y, z] = new Block(bd.matrix[x, y, z], pos, chunk.gameObject, this); // Crée un bloc à partir des données chargées
                            continue; // Passe à la prochaine itération de la boucle
                        }

                        int surfaceHeight = Utils.GenerateHeight(worldX, worldZ); // Génère la hauteur de la surface à partir des coordonnées x et z

                        if (worldY == 0) { // Si le bloc est à la hauteur 0 (niveau le plus bas)
                            chunkData[x, y, z] = new Block(Block.BlockType.BEDROCK, pos, chunk.gameObject, this); // Crée un bloc de type BEDROCK
                        } else if (worldY <= Utils.GenerateStoneHeight(worldX, worldZ)) { // Si le bloc est sous la hauteur de la couche de pierre
                            if (Utils.fBM3D(worldX, worldY, worldZ, 0.01f, 2) < 0.4f && worldY < 40) { // Condition basée sur une fonction de bruit pour générer des diamants
                                chunkData[x, y, z] = new Block(Block.BlockType.DIAMOND, pos, chunk.gameObject, this); // Crée un bloc de type DIAMOND
                            } else if (Utils.fBM3D(worldX, worldY, worldZ, 0.03f, 3) < 0.41f && worldY < 20) { // Condition basée sur une fonction de bruit pour générer de la redstone
                                chunkData[x, y, z] = new Block(Block.BlockType.REDSTONE, pos, chunk.gameObject, this); // Crée un bloc de type REDSTONE
                            } else {
                                chunkData[x, y, z] = new Block(Block.BlockType.STONE, pos, chunk.gameObject, this); // Crée un bloc de type STONE
                            }
                        } else if (worldY == surfaceHeight) { // Si le bloc est à la hauteur de la surface
                            if (Utils.fBM3D(worldX, worldY, worldZ, 0.4f, 2) < 0.4f) { // Condition basée sur une fonction de bruit pour générer une base en bois
                                chunkData[x, y, z] = new Block(Block.BlockType.WOODBASE, pos, chunk.gameObject, this); // Crée un bloc de type WOODBASE
                            } else {
                                chunkData[x, y, z] = new Block(Block.BlockType.GRASS, pos, chunk.gameObject, this); // Crée un bloc de type GRASS
                            }
                        } else if (worldY < surfaceHeight) { // Si le bloc est en dessous de la surface
                            chunkData[x, y, z] = new Block(Block.BlockType.DIRT, pos, chunk.gameObject, this); // Crée un bloc de type DIRT
                        } else if (worldY < 65) { // Si le bloc est en dessous de la hauteur 65
                            chunkData[x, y, z] = new Block(Block.BlockType.WATER, pos, fluid.gameObject, this); // Crée un bloc de type WATER
                        } else {
                            chunkData[x, y, z] = new Block(Block.BlockType.AIR, pos, chunk.gameObject, this); // Crée un bloc de type AIR
                        }

                        if (chunkData[x, y, z].bType != Block.BlockType.WATER && Utils.fBM3D(worldX, worldY, worldZ, 0.1f, 3) < 0.42f) { // Condition supplémentaire basée sur une fonction de bruit
                            chunkData[x, y, z] = new Block(Block.BlockType.AIR, pos, chunk.gameObject, this); // Remplace le bloc par un bloc de type AIR
                        }

                        status = ChunkStatus.DRAW; // Met à jour le statut du chunk à DRAW
                    }
                }
            }
    }


    public void Redraw() {
        GameObject.DestroyImmediate(chunk.GetComponent<MeshFilter>());
        GameObject.DestroyImmediate(chunk.GetComponent<MeshRenderer>());
        GameObject.DestroyImmediate(chunk.GetComponent<Collider>());
        GameObject.DestroyImmediate(fluid.GetComponent<MeshFilter>());
        GameObject.DestroyImmediate(fluid.GetComponent<MeshRenderer>());
        GameObject.DestroyImmediate(fluid.GetComponent<Collider>());
        DrawChunk();
    }

    public void DrawChunk() {
        if (!treesCreated) { // Si les arbres n'ont pas encore été créés
            for (int z = 0; z < World.chunkSize; z++) { // Parcourt les coordonnées z du chunk
                for (int y = 0; y < World.chunkSize; y++) { // Parcourt les coordonnées y du chunk
                    for (int x = 0; x < World.chunkSize; x++) { // Parcourt les coordonnées x du chunk
                        BuildTrees(chunkData[x, y, z], x, y, z); // Construit les arbres pour chaque bloc
                    }
                }
            }
            treesCreated = true; // Les arbres ont été créés
        }

        for (int z = 0; z < World.chunkSize; z++) { // Parcourt les coordonnées z du chunk
            for (int y = 0; y < World.chunkSize; y++) { // Parcourt les coordonnées y du chunk
                for (int x = 0; x < World.chunkSize; x++) { // Parcourt les coordonnées x du chunk
                    chunkData[x, y, z].Draw(); // Dessine chaque bloc du chunk
                }
            }
        }

        CombineQuads(chunk.gameObject, cubeMaterial); // Combine les maillages des blocs pour former un seul maillage pour le chunk
        MeshCollider collider = chunk.gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider; // Ajoute un composant MeshCollider pour la collision
        collider.sharedMesh = chunk.transform.GetComponent<MeshFilter>().mesh; // Associe le maillage combiné au MeshCollider

        CombineQuads(fluid.gameObject, fluidMaterial); // Combine les maillages des blocs d'eau pour former un seul maillage pour le fluide
        status = ChunkStatus.DONE; // Met à jour le statut du chunk à DONE
    }


    //Cette fonction sert à générer les arbres sur les terrains de verdure
    void BuildTrees(Block trunk, int x, int y, int z) {
        if (trunk.bType != Block.BlockType.WOODBASE) return; // Si le bloc de base n'est pas de type WOODBASE, la fonction se termine

        Block t = trunk.GetBlock(x, y + 1, z); // Récupère le bloc au-dessus du tronc
        if (t != null) {
            t.SetType(Block.BlockType.WOOD); // Change le type du bloc au-dessus du tronc en WOOD

            Block t1 = t.GetBlock(x, y + 2, z); // Récupère le bloc au-dessus du bloc précédent
            if (t1 != null) {
                t1.SetType(Block.BlockType.WOOD); // Change le type du bloc au-dessus en WOOD

                for (int i = -1; i <= 1; i++) { // Parcourt les coordonnées i de -1 à 1
                    for (int j = -1; j <= 1; j++) { // Parcourt les coordonnées j de -1 à 1
                        for (int k = 3; k <= 4; k++) { // Parcourt les coordonnées k de 3 à 4
                            Block t2 = trunk.GetBlock(x + i, y + k, z + j); // Récupère le bloc aux coordonnées spécifiées

                            if (t2 != null) {
                                t2.SetType(Block.BlockType.LEAVES); // Change le type du bloc récupéré en LEAVES (feuilles)
                                if (t2.owner.chunk.name == "0_112_0") {
                                    Debug.Log("Trunk At: " + trunk.owner.chunk.name);
                                    Debug.Log("Current Block " + trunk.position);
                                    Debug.Log("Trying for: " + (x + i) + " " + (y + k) + " " + (z + j));
                                }
                            } else {
                                return; // Si le bloc n'existe pas, la fonction se termine
                            }
                        }
                    }
                }

                Block t3 = t1.GetBlock(x, y + 5, z); // Récupère le bloc au-dessus du bloc précédent
                if (t3 != null) {
                    t3.SetType(Block.BlockType.LEAVES); // Change le type du bloc au-dessus en LEAVES (feuilles)
                }
            }
        }
    }

    public Chunk() { }

    // Constructeur surchargé avec des paramètres
    public Chunk(Vector3 position, Material c, Material t) {
        // Crée un nouvel objet GameObject pour représenter le chunk
        chunk = new GameObject(World.BuildChunkName(position));
        chunk.transform.position = position;

        // Crée un nouvel objet GameObject pour représenter le fluide
        fluid = new GameObject(World.BuildChunkName(position) + "_F");
        fluid.transform.position = position;

        // Ajoute un composant ChunkMB à l'objet chunk et définit son propriétaire sur cette instance de Chunk
        mb = chunk.AddComponent<ChunkMB>();
        mb.SetOwner(this);

        // Définit les matériaux pour les blocs solides (cubeMaterial) et les blocs fluides (fluidMaterial)
        cubeMaterial = c;
        fluidMaterial = t;

        // Appelle la fonction BuildChunk pour construire le contenu du chunk
        BuildChunk();
    }



    public void CombineQuads(GameObject o, Material m) {
        // 1. Combinez tous les maillages enfants
        MeshFilter[] meshFilters = o.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        // 2. Crée un nouveau maillage sur l'objet parent
        MeshFilter mf = (MeshFilter)o.gameObject.AddComponent(typeof(MeshFilter));
        mf.mesh = new Mesh();

        // 3. Ajoute les maillages combinés des enfants en tant que maillage du parent
        mf.mesh.CombineMeshes(combine);

        // 4. Crée un rendu pour le parent
        MeshRenderer renderer = o.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = m;

        // 5. Supprime tous les enfants non combinés
        foreach (Transform quad in o.transform) {
            GameObject.Destroy(quad.gameObject);
        }
    }


}
