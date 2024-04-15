using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MusikMacher
{
  // ensures that only one of the browse instances play at once.
  public class PlayManager
  {
    public static PlayManager Instance = new PlayManager();

    public PlayerModel? currentPlayer = null;
      
    public PlayManager() {
      currentPlayer = null;
    }

    public void Play(PlayerModel player)
    {
      // stop current and start the requested one.
      if(currentPlayer != null) {
        currentPlayer.Pause();
      }
      currentPlayer = player;
      player.DoPlay(); // do the actual playback
    }
  }
}
