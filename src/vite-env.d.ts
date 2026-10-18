/// <reference types="vite/client" />

interface ImportMetaEnv { readonly VITE_API_URL?: string; readonly VITE_USE_MOCKS?: string; readonly VITE_ENABLE_CLIENT_AUTH?: string }
interface ImportMeta { readonly env: ImportMetaEnv }
