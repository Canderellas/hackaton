INSERT INTO public."Device_style_value" ("FK_Device_property_value", "FK_Style_property", "Value") VALUES
-- Группа 1-6: Разные цветовые схемы и эффекты
(1, 1, '#2c3e50'),
(1, 4, 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'),
(1, 18, '15px'),
(1, 19, '0 10px 30px rgba(0,0,0,0.3)'),
(1, 21, 'scale(1.02)'),
(1, 22, 'brightness(1.1)'),

(2, 1, '#27ae60'),
(2, 2, '18px'),
(2, 3, '800'),
(2, 7, '20px 15px'),
(2, 26, 'Arial, sans-serif'),
(2, 28, '2px'),

(3, 1, '#e74c3c'),
(3, 4, '#fef9e7'),
(3, 5, '3px dashed #f39c12'),
(3, 17, 'underline overline'),
(3, 29, 'uppercase'),
(3, 41, 'none'),

-- Группа 7-12: Flex и Grid layouts
(4, 11, 'flex'),
(4, 37, 'column'),
(4, 38, 'center'),
(4, 39, 'stretch'),
(4, 40, 'wrap'),
(4, 41, '15px'),

(5, 11, 'grid'),
(5, 45, 'repeat(auto-fit, minmax(150px, 1fr))'),
(5, 46, 'auto'),
(5, 47, 'span 2'),
(5, 48, '1'),
(5, 49, 'cover'),

(6, 1, '#8e44ad'),
(6, 4, 'radial-gradient(circle, #8e44ad, #9b59b6)'),
(6, 13, '0.95'),
(6, 22, 'sepia(0.2)'),
(6, 43, 'blur(10px)'),
(6, 44, 'multiply'),

-- Группа 13-18: Текст и типографика
(7, 1, '#34495e'),
(7, 2, '16px'),
(7, 26, 'Georgia, serif'),
(7, 27, '1.8'),
(7, 28, '1px'),
(7, 29, 'capitalize'),

(8, 1, '#c0392b'),
(8, 3, '600'),
(8, 6, 'justify'),
(8, 17, 'line-through'),
(8, 30, 'nowrap'),
(8, 31, 'hidden'),

(9, 1, '#16a085'),
(9, 4, '#ecf0f1'),
(9, 7, '25px'),
(9, 8, '10px 5px'),
(9, 18, '50px 0 50px 0'),
(9, 19, 'inset 0 0 10px rgba(0,0,0,0.1)'),

-- Группа 19-24: Специальные эффекты
(10, 1, '#d35400'),
(10, 4, 'url("https://example.com/texture.png")'),
(10, 23, 'cover'),
(10, 24, 'center'),
(10, 42, 'polygon(0 0, 100% 0, 100% 85%, 0 100%)'),
(10, 52, 'pan-y')
ON CONFLICT DO NOTHING;