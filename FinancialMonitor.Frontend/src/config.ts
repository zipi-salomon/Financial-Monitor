interface AppConfig {
  apiBaseUrl: string;
}

let config: AppConfig = {
  apiBaseUrl: 'http://localhost:5000' // ברירת מחדל ליתר ביטחון
};

export const loadConfig = async (): Promise<AppConfig> => {
  try {
    const response = await fetch('/config.json');
    if (response.ok) {
      config = await response.json();
    }
  } catch (error) {
    console.error('Failed to load config.json, using fallback:', error);
  }
  return config;
};

export const getConfig = (): AppConfig => config;