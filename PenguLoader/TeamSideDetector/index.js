/*
 * @author Kubi
 * @link https://github.com/Ku-Tadao/IPostWeirdStuffHere/tree/master/PenguLoader/TeamSideDetector
 * @description Shows team side on screen when holding Ctrl+Shift in champion select.
 */

// --- Module-scoped variables ---
let isInChampSelect = false;
let currentTeam = null; // Can be 'BLUE' or 'RED'
let notification = null; // The UI element

// --- UI Helper Functions ---
const showNotification = (team) => {
  if (!notification) return;
  notification.className = `team-side-notification ${team.toLowerCase()}`;
  notification.textContent = `${team} Side`;
  notification.classList.add('visible');
};

const hideNotification = () => {
  if (notification) {
    notification.classList.remove('visible');
  }
};

// --- Logic & Event Handlers ---
const determineTeamSide = async () => {
  try {
    const response = await fetch('./lol-champ-select/v1/session');
    if (response.ok) {
      const data = await response.json();
      const localPlayer = data.myTeam.find(p => p.cellId === data.localPlayerCellId);
      currentTeam = localPlayer?.team === 1 ? 'BLUE' : 'RED';
    }
  } catch (error) {
    currentTeam = null;
    console.error("TeamSideDetector Error: Failed to determine team side.", error);
  }
};

const onGameflowPhaseChange = (event) => {
    const newPhase = event.data;
    const wasInChampSelect = isInChampSelect;
    isInChampSelect = (newPhase === 'ChampSelect');

    if (isInChampSelect && !wasInChampSelect) {
        determineTeamSide();
    } else if (!isInChampSelect && wasInChampSelect) {
        currentTeam = null;
        hideNotification();
    }
};

const handleKeyDown = (event) => {
  if (event.ctrlKey && event.shiftKey && isInChampSelect && currentTeam) {
    showNotification(currentTeam);
  }
};

const handleKeyUp = (event) => {
  // Hide the notification if either Control or Shift is released.
  if (event.key === 'Control' || event.key === 'Shift') {
    hideNotification();
  }
};

// --- Initial State Check ---
const checkInitialState = async () => {
    try {
        const res = await fetch('/lol-gameflow/v1/gameflow-phase');
        if (res.ok) {
            const phase = await res.json();
            if (phase === 'ChampSelect') {
                isInChampSelect = true;
                determineTeamSide();
            }
        }
    } catch (e) { /* Fail silently on initial check */ }
};

// --- PenguLoader Entry Points ---

/**
 * init() is used for core logic and LCU API subscriptions.
 */
export function init(context) {
    context.socket.observe('/lol-gameflow/v1/gameflow-phase', onGameflowPhaseChange);
    checkInitialState();
}

/**
 * load() is used for creating UI elements and attaching DOM/window event listeners.
 */
export function load() {
    // Use a MutationObserver to safely wait for the client's main UI root to appear.
    const observer = new MutationObserver((mutations, obs) => {
        const viewportRoot = document.getElementById('rcp-fe-viewport-root');
        
        // Once the viewport exists, inject the UI and stop observing.
        if (viewportRoot) {
            // 1. Inject CSS styles into the document's head.
            const style = document.createElement('style');
            style.textContent = `
              .team-side-notification {
                position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%);
                padding: 20px 40px; border-radius: 8px; font-size: 24px; font-weight: bold;
                color: white; z-index: 9999; opacity: 0; transition: opacity 0.2s;
                pointer-events: none;
              }
              .team-side-notification.blue {
                background: rgba(0, 48, 163, 0.9); box-shadow: 0 0 20px rgba(0, 148, 255, 0.5);
              }
              .team-side-notification.red {
                background: rgba(163, 0, 0, 0.9); box-shadow: 0 0 20px rgba(255, 0, 0, 0.5);
              }
              .team-side-notification.visible { opacity: 1; }
            `;
            document.head.appendChild(style);

            // 2. Create the notification element and append it to the correct UI layer.
            notification = document.createElement('div');
            notification.className = 'team-side-notification';
            viewportRoot.appendChild(notification);

            // 3. Set up the keyboard listeners.
            document.addEventListener('keydown', handleKeyDown);
            document.addEventListener('keyup', handleKeyUp);
            
            // 4. Also hide the notification if the user clicks away from the window.
            window.addEventListener('blur', hideNotification);

            // 5. We're done, so disconnect the observer to save resources.
            obs.disconnect();
        }
    });

    // Start observing the document body for added child elements.
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
}