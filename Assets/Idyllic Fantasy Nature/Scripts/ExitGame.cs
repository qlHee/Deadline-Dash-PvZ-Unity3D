using UnityEngine;

namespace IdyllicFantasyNature
{
    public class ExitGame : MonoBehaviour
    {
        void Update()
        {
            // If you press the ESC key in the game, the application will be closed
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}
