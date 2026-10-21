import sanitizeHtml from 'sanitize-html';


export const formatBodyText = (text) => {
  if (!text) return text;

  // Genera <img> con: ![alt](url)
  const withImages = text.replace(
    /!\[([^\]]*)\]\(([^)]+)\)/g,
    (match, alt, url) => {
      return `<img src="${url}" alt="${alt}" class="embedded-image" style="max-width:100%;max-height:400px;border-radius:8px;margin-top:8px;" loading="lazy" />`;
    }
  );

  // Genera <a> con: [texto](url)
  const withLinks = withImages.replace(
    /\[([^\]]+)\]\(([^)]+)\)/g,
    (match, text, url) => {
      return `<a href="${url}" target="_blank" rel="noopener noreferrer" class="content-link">${text}</a>`;
    }
  );

// Esta librería (sanitize-html) teóricamente asegura que no haya código potencialmente malicioso en el html generado.
  const sanitized = sanitizeHtml(withLinks, {
    allowedTags: ['a', 'img'],
    allowedAttributes: {
        a:['href', 'target', 'rel'],
        img:['src', 'alt', 'class', 'style']
    }
  })

  return sanitized;
  
};