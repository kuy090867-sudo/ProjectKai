using UnityEngine;
using TMPro;

namespace ProjectKai.UI
{
    public class DamagePopup : MonoBehaviour
    {
        private TextMeshPro _text;
        private float _lifetime = 0.8f;
        private float _timer;
        private Vector3 _velocity;

        public static DamagePopup Create(Vector3 position, float damage, Color color)
        {
            var obj = new GameObject("DamagePopup");
            obj.transform.position = position + new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(0.3f, 0.6f),
                0f);

            var tmp = obj.AddComponent<TextMeshPro>();
            tmp.text = Mathf.RoundToInt(damage).ToString();
            tmp.fontSize = 4f;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 20;

            var popup = obj.AddComponent<DamagePopup>();
            popup._text = tmp;
            popup._velocity = new Vector3(Random.Range(-0.5f, 0.5f), 2f, 0f);

            return popup;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            transform.position += _velocity * Time.deltaTime;
            _velocity.y -= 3f * Time.deltaTime;

            // 페이드 아웃
            if (_text != null)
            {
                float alpha = 1f - (_timer / _lifetime);
                var c = _text.color;
                c.a = Mathf.Max(alpha, 0f);
                _text.color = c;
            }

            if (_timer >= _lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
