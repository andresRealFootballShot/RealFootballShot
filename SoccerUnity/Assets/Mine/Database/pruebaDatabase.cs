using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MySql.Data.MySqlClient;
//Ref https://www.youtube.com/watch?v=2CPr_wASXNI&t=642s
//https://www.youtube.com/watch?v=rJHtOGv3BzQ
//Para que funcione en los builds tiene que tener el .Net 4.x profile 
public class pruebaDatabase : MonoBehaviour
{
    string datosConexion;
    string servidroBaeDatos, nombreBaseDtos, usuarioBaseDatos, contraseniaBaseDatos;
    MySqlConnection conexion;
    void Start()
    {
        //StartCoroutine(prueba());
        datosConexion = "Server=sql11.freemysqlhosting.net;" + "Database=sql11433499;"
            + "Uid=sql11433499;" +
            "Pwd=v2jTyBnWbP;";
        Conectar();
        string comand1 = "INSERT INTO `Usuarios` (`name`) VALUES ('Antonio')";
        string comand2 = "SELECT * FROM `Usuarios` WHERE `name` LIKE 'Pepito'";
        //SendComand(comand1);
        //MySqlDataReader result = SendComand(comand2);

    }
    void Conectar()
    {
        conexion = new MySqlConnection(datosConexion);
        try
        {
            conexion.Open();
        }
        catch(MySqlException e)
        {
            print("conexion erronea:"+e);
        }
    }
    public MySqlDataReader SendComand(string comand)
    {
        MySqlCommand cmd = conexion.CreateCommand();
        cmd.CommandText = comand;
        MySqlDataReader resultado = cmd.ExecuteReader();
        return resultado;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*IEnumerator prueba()
    {
        
        WWWForm form = new WWWForm();
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
        }
    }
    */
}
