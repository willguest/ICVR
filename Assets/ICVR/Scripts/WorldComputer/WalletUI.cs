using Newtonsoft.Json;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ICVR
{
    public class WalletUI : MonoBehaviour
    {
        [SerializeField] private Transform spawnLocation;
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private Text screenText;

        private StringBuilder sb;
        private bool isPrinting = false;


        private void Start()
        {
            sb = new StringBuilder();
            screenText.text = "";
            UpdateScreenMessage("Initialising...");
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.ScrollLock))
            {
                RequestCoinFromFund();
            }
        }

        /// <summary>
        /// Entry point for requesting a token from the ledger canister.
        /// </summary>
        public void BeginTokenRequest()
        {
            var profile = ChainAPI.Instance.currentProfile;

            // for testing
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                string iName = "pr2du-4rav3-30gvv-ielbu-n135t-k3pw7-72fgw-mpqiq-3oa2b-exqzn-sae";
                string bookd = iName.BookEnd(7, "...");
                UpdateScreenMessage("Requesting test with: " + bookd, onGetCoin);
                return;
            }
            
            if (profile == null || profile.principal.Length != 63)
            {
                UpdateScreenMessage("Not yet authenticated (code " + profile.principal.Length + ")");
                return;
            }

            string shortP = profile.principal.BookEnd(7, "...");
            UpdateScreenMessage("Requesting coin with: " + shortP, RequestCoinFromFund);
        }


        private void RequestCoinFromFund(string message = "")
        {
            ChainAPI.Instance.RequestToken(onGetCoin);
        }

        private void onGetCoin(string jsonData)
        {
            CoinRequestResponse response = new CoinRequestResponse();

            // testing with null strings
            if (string.IsNullOrEmpty(jsonData)
                 && Application.platform == RuntimePlatform.WindowsEditor)
            {
                GetComponent<AudioSource>().Play();
                UpdateScreenMessage("Test complete" + System.Environment.NewLine +
                        "Please have a coin.", SpawnCoin);
                return;
            }

            try
            {
                response = JsonConvert.DeserializeObject<CoinRequestResponse>(jsonData);
                if (response != null)
                {
                    GetComponent<AudioSource>().Play();
                    UpdateScreenMessage("Accepted    " + System.Environment.NewLine +
                        "Your balance is: " + ( response.fundCount / 100 ) + ".00", SpawnCoin);
                }

            }
            catch (System.Exception err)
            {
                UpdateScreenMessage("Something has gone awry:\n" + err);
            }

            ChainAPI.Instance.FinaliseCallback(response.cbIndex);
        }

        private void SpawnCoin(string param = "")
        {
            Vector3 spawnPosition = transform.TransformPoint(spawnLocation.localPosition);
            Instantiate(coinPrefab, spawnPosition, Quaternion.identity * Quaternion.AngleAxis(135f, Vector3.up), null);
        }

        private void UpdateScreenMessage(string appendage, System.Action<string> callback = null, string param = "")
        {
            while (sbLen >= 7)
            {
                screenText.text =  RemoveFirstLine();
                sbLen--;
            }

            if (isPrinting) return;

            StartCoroutine(PrintString(appendage, callback, param));
        }

        private int sbLen = 0;

        string RemoveFirstLine()
        {
            sb.Remove(0, sb.ToString().IndexOf(System.Environment.NewLine) + 1);
            return sb.ToString();
        }

        private IEnumerator PrintString(string _input, System.Action<string> cb = null, string param = "")
        {
            isPrinting = true;
            foreach (char c in _input.ToCharArray())
            {
                sb.Append(c);
                screenText.text = sb.ToString();
                if (c.Equals('.'))
                {
                    yield return new WaitForSeconds(0.7f);
                }
                else if (c.Equals(System.Environment.NewLine.ToCharArray()[0]))
                {
                    sbLen++;
                    yield return new WaitForSeconds(0.12f);
                }
                else
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
            sb.Append(System.Environment.NewLine);
            sbLen++;
            isPrinting = false;
            cb?.Invoke(param);
        }

    }
}