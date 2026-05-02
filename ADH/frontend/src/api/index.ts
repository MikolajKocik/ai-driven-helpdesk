const API_BASE = 'http://localhost:5033/api';

export const authApi = {
  login: async (username: string, password: string) => {
    const res = await fetch(`${API_BASE}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });
    if (!res.ok) throw new Error('Login failed');
    return res.json();
  }
};

export const ticketsApi = {
  getTickets: async (token: string) => {
    const res = await fetch(`${API_BASE}/tickets`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error('Failed to fetch tickets');
    return res.json();
  },
  createTicket: async (token: string, description: string) => {
    const res = await fetch(`${API_BASE}/tickets`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ description })
    });
    if (!res.ok) throw new Error('Failed to create ticket');
    return res.json();
  }
};

export const chatApi = {
  streamChat: async (token: string, messages: any[]) => {
    return fetch(`${API_BASE}/chat`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ messages })
    });
  }
};

export const articlesApi = {
  getArticles: async (token: string) => {
    const res = await fetch(`${API_BASE}/articles`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    return res.json();
  },
  createArticle: async (token: string, article: { title: string, content: string }) => {
    const res = await fetch(`${API_BASE}/articles`, {
      method: 'POST',
      headers: { 
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}` 
      },
      body: JSON.stringify(article)
    });
    return res.json();
  },
  deleteArticle: async (token: string, id: string) => {
    await fetch(`${API_BASE}/articles/${id}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` }
    });
  }
};
