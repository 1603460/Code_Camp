using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DynamicCodeExecutor : MonoBehaviour
{
    public TMP_InputField codeInputField;
    public Button executeButton;
    public GameObject character;
    public float moveSpeed = 2.0f;
    public int level;
    private int asks;

    public GameObject victoryPanel;
    public GameObject failPanel;
    public GameObject levelPanel;

    private Rigidbody2D characterRigidbody;
    private Collider2D characterCollider;
    public Transform movePoint;
    public LayerMask stopMovement;

    private bool isMoving = false;

    void Start()
    {
        executeButton.onClick.AddListener(OnExecuteButtonClicked);
        characterRigidbody = character.GetComponent<Rigidbody2D>();
        characterCollider = character.GetComponent<Collider2D>();
        movePoint.parent = null;
        asks = 0;
    }

    void OnExecuteButtonClicked()
    {
        string code = TransformUserCode(codeInputField.text);
        Debug.Log($"Código transformado: {code}");
        ExecuteCode(code).ConfigureAwait(false);
    }

    async Task ExecuteCode(string code)
    {
        Debug.Log("Iniciando ejecución de código dinámico...");

        string fullCode = $@"
        using System;
        using UnityEngine;
        using System.Threading.Tasks;

        public class Script
        {{
            private DynamicCodeExecutor hero;
            private GameObject character;

            public Script(DynamicCodeExecutor hero, GameObject character)
            {{
                this.hero = hero;
                this.character = character;
            }}

            public async Task Execute()
            {{
                try
                {{
                    {code}
                }}
                catch (Exception ex)
                {{
                    Debug.LogError($""Excepción durante la ejecución del script:"");
                    Debug.LogError(ex.StackTrace);
                }}
            }}
        }}";

        Debug.Log($"Código completo a compilar:\n{fullCode}");

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fullCode);

        Debug.Log("arbol creado...");
        string assemblyName = Path.GetRandomFileName();
        Debug.Log("assembly creado...");
        Debug.Log(Path.Combine(Application.streamingAssetsPath, "netstandard.dll"));
        MetadataReference[] references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            //MetadataReference.CreateFromFile(typeof(MonoBehaviour).Assembly.Location),
            //MetadataReference.CreateFromFile(typeof(GameObject).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Debug).Assembly.Location),
            //MetadataReference.CreateFromFile(typeof(Vector3).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(Application.streamingAssetsPath, "netstandard.dll")),
            MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location),
        };
        Debug.Log("Iniciando compilación...");
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using (var ms = new MemoryStream())
        {
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                Debug.LogError("Error en la compilación del código dinámico.");
                foreach (Diagnostic diagnostic in result.Diagnostics)
                {
                    Debug.LogError($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }
                return;
            }

            ms.Seek(0, SeekOrigin.Begin);
            byte[] assemblyData = ms.ToArray();

            try
            {
                Assembly assembly = Assembly.Load(assemblyData);
                Debug.Log("Asamblea cargada correctamente.");

                Type type = assembly.GetType("Script");
                if (type == null)
                {
                    Debug.LogError("No se encontró la clase Script en la asamblea.");
                    return;
                }

                object obj = Activator.CreateInstance(type, this, character);
                if (obj == null)
                {
                    Debug.LogError("No se pudo crear una instancia de la clase Script.");
                    return;
                }

                MethodInfo executeMethod = type.GetMethod("Execute");
                if (executeMethod == null)
                {
                    Debug.LogError("No se encontró el método Execute en la clase Script.");
                    return;
                }

                Debug.Log("Invocando método Execute en clase Script...");
                Task task = (Task)executeMethod.Invoke(obj, null);
                await task;
                Debug.Log("Método Execute completado.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error al cargar o ejecutar la asamblea: {ex.Message}");
                Debug.LogError(ex.StackTrace);
            }
        }
    }

    string TransformUserCode(string code)
    {
        var lines = code.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var transformedLines = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (trimmedLine.StartsWith("hero.Move"))
            {
                var indent = line.Substring(0, line.IndexOf(trimmedLine));
                transformedLines.Add($"{indent}await {trimmedLine};");
            }
            else
            {
                transformedLines.Add(line);
            }
        }
        Debug.Log($"Código transformado: {string.Join("\n", transformedLines)}");
        return string.Join("\n", transformedLines);
    }

    public async Task MoveCharacterToPoint(Vector3 targetPosition)
    {
        Debug.Log($"Moviendo personaje a la posición: {targetPosition}");
        isMoving = true;
        while (Vector3.Distance(character.transform.position, targetPosition) > 0.001f)
        {
            character.transform.position = Vector3.MoveTowards(character.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            await Task.Yield();
        }
        character.transform.position = targetPosition;
        isMoving = false;
        Debug.Log("Movimiento del personaje completado.");
    }

    public async Task QueueMove(Vector3 direction, int steps)
    {
        Debug.Log($"Iniciando movimiento en dirección: {direction} por {steps} pasos.");
        for (int i = 0; i < steps; i++)
        {
            if (!Physics2D.OverlapCircle(movePoint.position + direction, .2f, stopMovement))
            {
                movePoint.position += direction;
                Debug.Log($"Nueva posición del punto de movimiento: {movePoint.position}");
            }
            else
            {
                Debug.Log("Movimiento bloqueado");
                break;
            }
        }
        Vector3 finalPosition = movePoint.position;
        finalPosition.y += 0.5f;
        await MoveCharacterToPoint(finalPosition);
    }

    public async Task MoveRight(int steps)
    {
        Debug.Log("Ejecutando MoveRight...");
        await QueueMove(Vector3.right, steps);
    }

    public async Task MoveLeft(int steps)
    {
        Debug.Log("Ejecutando MoveLeft...");
        await QueueMove(Vector3.left, steps);
    }

    public async Task MoveUp(int steps)
    {
        Debug.Log("Ejecutando MoveUp...");
        await QueueMove(Vector3.up, steps);
    }

    public async Task MoveDown(int steps)
    {
        Debug.Log("Ejecutando MoveDown...");
        await QueueMove(Vector3.down, steps);
    }

    public int askAge()
    {
        if (level == 1) 
        {
            Vector3 currentPosition = character.transform.position;
            Debug.Log($"Posición actual del personaje: {currentPosition}");

            if (currentPosition == new Vector3(-1, 4, 0))
            {
                return 10; 
            } 
            else if (currentPosition == new Vector3(-7, -2, 0))
            {
                return 8;
            }
            else if (currentPosition == new Vector3(5, -4, 0))
            {
                return 14;
            }
            else if (currentPosition == new Vector3(-11, -7, 0))
            {
                return 7;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return 0;
        } 
    }
    public string askItem() 
    {
        Vector3 currentPosition = character.transform.position;
        if (currentPosition == new Vector3(0, 3, 0))
        {
            System.Random random = new System.Random();
        
            int randomNumber = random.Next(0, 2);

            // Devolver "Sandwich" si el número es 0, de lo contrario devolver "Rose"
            if(level == 2)
            {
                return randomNumber == 0 ? "Sandwich" : "Rose";
            }
            else
            {
                if(level == 3)
                {
                    return randomNumber == 0 ? "Toy" : "Rose";
                }
                else
                {
                    return "";
                }
            }
        }
        else
        {
            return "";
        }
    }

    public void respond(float response)
    {
        if (level == 1)
        {
            if(response == 9.75f)
            {
                levelPanel.SetActive(false);
                victoryPanel.SetActive(true);
            }
            else
            {
                Debug.Log("Respuesta incorrecta");
                levelPanel.SetActive(false);
                failPanel.SetActive(true);
            }
        }
        if (level == 2)
        {

        }
    }

    public void give(string item)
    {
        Vector3 currentPosition = character.transform.position;
        
        if(item == "Rose")
        {
            if(currentPosition == new Vector3(4, -4, 0))
            {
                if(level == 2)
                {
                    levelPanel.SetActive(false);
                    victoryPanel.SetActive(true);
                }
                if(level == 3)
                {
                    asks += 1;
                    if(asks == 5)
                    {
                        levelPanel.SetActive(false);
                        victoryPanel.SetActive(true);
                    }
                }
            }
            else
            {
                Debug.Log("Posicion incorrecta");
                levelPanel.SetActive(false);
                failPanel.SetActive(true);
            }
        }
        if(item == "Sandwich")
        {
            if(currentPosition == new Vector3(-9, 1, 0))
            {
                if(level == 2)
                {
                    levelPanel.SetActive(false);
                    victoryPanel.SetActive(true);
                }
                if(level == 3)
                {
                    asks += 1;
                    if(asks == 5)
                    {
                        levelPanel.SetActive(false);
                        victoryPanel.SetActive(true);
                    }
                }
            }
            else
            {
                Debug.Log("Posicion incorrecta");
                levelPanel.SetActive(false);
                failPanel.SetActive(true);
            }
        }
        if(item == "Toy")
        {
            if(currentPosition == new Vector3(-5, 0, 0))
            {
                if(level == 2)
                {
                    levelPanel.SetActive(false);
                    victoryPanel.SetActive(true);
                }
                if(level == 3)
                {
                    asks += 1;
                    if(asks == 5)
                    {
                        levelPanel.SetActive(false);
                        victoryPanel.SetActive(true);
                    }
                }
            }
            else
            {
                Debug.Log("Posicion incorrecta");
                levelPanel.SetActive(false);
                failPanel.SetActive(true);
            }
        }
    }
}