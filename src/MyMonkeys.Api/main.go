package main

import (
	"crypto/rand"
	"crypto/rsa"
	"crypto/x509"
	"crypto/x509/pkix"
	"encoding/json"
	"errors"
	"fmt"
	"log"
	"math/big"
	"net/http"
	"net"
	"os"
	"path/filepath"
	"time"
	"encoding/pem"
	"strings"
)

type Monkey struct {
	ID         string  `json:"id"`
	Name       string  `json:"name"`
	Location   string  `json:"location"`
	Details    string  `json:"details"`
	ImageURL   string  `json:"imageUrl"`
	Population int     `json:"population"`
	Latitude   float64 `json:"latitude"`
	Longitude  float64 `json:"longitude"`
}

type App struct {
	MonkeysByID map[string]Monkey
	Monkeys     []Monkey
}

func main() {
	port := os.Getenv("PORT")
	if port == "" {
		port = "6001"
	}

	useHttps := strings.EqualFold(os.Getenv("USE_HTTPS"), "true") || os.Getenv("USE_HTTPS") == "1"
	certFile := os.Getenv("TLS_CERT_FILE")
	keyFile := os.Getenv("TLS_KEY_FILE")

	app, err := loadApp()
	if err != nil {
		log.Fatalf("failed to load dataset: %v", err)
	}

	mux := http.NewServeMux()
	mux.HandleFunc("GET /health", func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusOK)
		_, _ = w.Write([]byte("ok"))
	})

	mux.HandleFunc("GET /monkeys", func(w http.ResponseWriter, r *http.Request) {
		writeJSON(w, http.StatusOK, app.Monkeys)
	})

	mux.HandleFunc("GET /monkeys/{id}", func(w http.ResponseWriter, r *http.Request) {
		id := r.PathValue("id")
		monkey, ok := app.MonkeysByID[id]
		if !ok {
			http.NotFound(w, r)
			return
		}
		writeJSON(w, http.StatusOK, monkey)
	})

	addr := ":" + port
	if !useHttps {
		log.Printf("monkeys-service listening (http) on %s", addr)
		if err := http.ListenAndServe(addr, mux); err != nil {
			log.Fatal(err)
		}
		return
	}

	if certFile == "" || keyFile == "" {
		generatedCert, generatedKey, genErr := generateLocalhostCertFiles("mymonkeys-api")
		if genErr != nil {
			log.Fatalf("failed to generate TLS cert: %v", genErr)
		}
		certFile = generatedCert
		keyFile = generatedKey
	}

	log.Printf("monkeys-service listening (https) on %s", addr)
	if err := http.ListenAndServeTLS(addr, certFile, keyFile, mux); err != nil {
		log.Fatal(err)
	}
}

func generateLocalhostCertFiles(prefix string) (certPath string, keyPath string, err error) {
	key, err := rsa.GenerateKey(rand.Reader, 2048)
	if err != nil {
		return "", "", err
	}

	now := time.Now()
	serial, err := rand.Int(rand.Reader, new(big.Int).Lsh(big.NewInt(1), 128))
	if err != nil {
		return "", "", err
	}

	tmpl := x509.Certificate{
		SerialNumber: serial,
		Subject: pkix.Name{
			CommonName: "localhost",
		},
		NotBefore:             now.Add(-1 * time.Hour),
		NotAfter:              now.Add(7 * 24 * time.Hour),
		KeyUsage:              x509.KeyUsageKeyEncipherment | x509.KeyUsageDigitalSignature,
		ExtKeyUsage:           []x509.ExtKeyUsage{x509.ExtKeyUsageServerAuth},
		BasicConstraintsValid: true,
		DNSNames:              []string{"localhost"},
		IPAddresses:           []net.IP{net.ParseIP("127.0.0.1"), net.ParseIP("::1")},
	}

	der, err := x509.CreateCertificate(rand.Reader, &tmpl, &tmpl, &key.PublicKey, key)
	if err != nil {
		return "", "", err
	}

	dir := filepath.Join(os.TempDir(), "mymonkeys")
	if err := os.MkdirAll(dir, 0o755); err != nil {
		return "", "", err
	}

	certPath = filepath.Join(dir, fmt.Sprintf("%s-cert.pem", prefix))
	keyPath = filepath.Join(dir, fmt.Sprintf("%s-key.pem", prefix))

	certOut, err := os.OpenFile(certPath, os.O_WRONLY|os.O_CREATE|os.O_TRUNC, 0o644)
	if err != nil {
		return "", "", err
	}
	defer certOut.Close()
	if err := pem.Encode(certOut, &pem.Block{Type: "CERTIFICATE", Bytes: der}); err != nil {
		return "", "", err
	}

	keyOut, err := os.OpenFile(keyPath, os.O_WRONLY|os.O_CREATE|os.O_TRUNC, 0o600)
	if err != nil {
		return "", "", err
	}
	defer keyOut.Close()
	if err := pem.Encode(keyOut, &pem.Block{Type: "RSA PRIVATE KEY", Bytes: x509.MarshalPKCS1PrivateKey(key)}); err != nil {
		return "", "", err
	}

	return certPath, keyPath, nil
}

func loadApp() (*App, error) {
	dataPath, err := findDataFile("monkeys.json")
	if err != nil {
		return nil, err
	}

	b, err := os.ReadFile(dataPath)
	if err != nil {
		return nil, err
	}

	var monkeys []Monkey
	if err := json.Unmarshal(b, &monkeys); err != nil {
		return nil, err
	}

	byID := make(map[string]Monkey, len(monkeys))
	for _, m := range monkeys {
		if m.ID == "" {
			m.ID = slugify(m.Name)
		}
		byID[m.ID] = m
	}

	return &App{Monkeys: monkeys, MonkeysByID: byID}, nil
}

func findDataFile(filename string) (string, error) {
	// Try working directory first (local dev).
	cwd, _ := os.Getwd()
	candidate := filepath.Join(cwd, "data", filename)
	if _, err := os.Stat(candidate); err == nil {
		return candidate, nil
	}

	// Try alongside the executable (container/published).
	exe, err := os.Executable()
	if err == nil {
		exeDir := filepath.Dir(exe)
		candidate = filepath.Join(exeDir, "data", filename)
		if _, statErr := os.Stat(candidate); statErr == nil {
			return candidate, nil
		}
	}

	return "", errors.New("data file not found")
}

func writeJSON(w http.ResponseWriter, status int, v any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	enc := json.NewEncoder(w)
	enc.SetIndent("", "  ")
	_ = enc.Encode(v)
}

func slugify(s string) string {
	s = strings.TrimSpace(strings.ToLower(s))
	// Replace common separators with hyphens.
	s = strings.NewReplacer("_", "-", " ", "-", "&", "and").Replace(s)

	// Keep [a-z0-9-] only.
	var b strings.Builder
	b.Grow(len(s))
	lastHyphen := false
	for _, r := range s {
		isAZ := r >= 'a' && r <= 'z'
		is09 := r >= '0' && r <= '9'
		if isAZ || is09 {
			b.WriteRune(r)
			lastHyphen = false
			continue
		}
		if r == '-' {
			if !lastHyphen {
				b.WriteRune('-')
				lastHyphen = true
			}
		}
	}

	out := b.String()
	out = strings.Trim(out, "-")
	if out == "" {
		return "monkey"
	}
	return out
}
