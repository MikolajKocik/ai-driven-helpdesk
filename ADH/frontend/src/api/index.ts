import axios from 'axios';

const API_BASE = 'http://localhost:5000/api/v1';

export const api = axios.create({
  baseURL: API_BASE,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const authApi = {
  login: async (username: string, password: string) => {
    const res = await api.post('/auth/login', { username, password });
    return res.data;
  }
};

export const ticketsApi = {
  getTickets: async () => {
    const res = await api.get('/tickets');
    return res.data;
  },
  createTicket: async (description: string) => {
    const res = await api.post('/tickets', { description });
    return res.data;
  }
};

export const chatApi = {
  streamChat: async (messages: any[]) => {
    return fetch(`${API_BASE}/chat/stream`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      },
      body: JSON.stringify({ messages })
    });
  }
};

export const articlesApi = {
  getArticles: async () => {
    const res = await api.get('/articles');
    return res.data;
  },
  createArticle: async (article: { title: string, content: string }) => {
    const res = await api.post('/articles', article);
    return res.data;
  },
  deleteArticle: async (id: string) => {
    await api.delete(`/articles/${id}`);
  }
};
