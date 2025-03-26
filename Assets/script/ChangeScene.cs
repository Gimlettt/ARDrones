/**
 * @file ChangeScene.cs
 * @brief Provides functionality to change scenes.
 *
 * This class contains a method to load a new scene based on a provided scene ID.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    /**
     * @brief Changes the current scene.
     *
     * Loads the scene corresponding to the given scene ID.
     *
     * @param sceneID The ID of the scene to load.
     */
    public void ChangeToScene(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }
}
